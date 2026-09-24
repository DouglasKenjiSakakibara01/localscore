using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using LocalScore.Application.Authentication;
using LocalScore.Application.Common;
using LocalScore.Infrastructure.Identity;
using LocalScore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LocalScore.Infrastructure.Authentication;

internal sealed class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    LocalScoreDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : IAuthenticationService
{
    private const string RotatedReason = "rotated";
    private const string LogoutReason = "logout";
    private const string ReuseReason = "reuse_detected";
    private readonly JwtOptions _options = options.Value;

    public async Task<Result<AuthenticationSession>> RegisterAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        var name = command.Name.Trim();
        var email = command.Email.Trim();
        var validationErrors = ValidateRegistration(name, email, command.Password);

        if (validationErrors.Count > 0)
        {
            return Result<AuthenticationSession>.Failure(
                AuthenticationErrors.InvalidRegistration(validationErrors));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            UserName = email,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true,
            LockoutEnabled = true
        };

        var creationResult = await userManager.CreateAsync(user, command.Password);

        if (!creationResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MapCreationFailure(creationResult);
        }

        var session = await CreateSessionAsync(user, Guid.NewGuid(), now, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<AuthenticationSession>.Success(session);
    }

    public async Task<Result<AuthenticationSession>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            return Result<AuthenticationSession>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result<AuthenticationSession>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        if (!await userManager.CheckPasswordAsync(user, command.Password))
        {
            await userManager.AccessFailedAsync(user);
            return Result<AuthenticationSession>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var now = timeProvider.GetUtcNow();
        await RemoveExpiredTokensAsync(user.Id, now, cancellationToken);
        var session = await CreateSessionAsync(user, Guid.NewGuid(), now, cancellationToken);

        return Result<AuthenticationSession>.Success(session);
    }

    public async Task<Result<AuthenticationSession>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result<AuthenticationSession>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        var tokenHash = RefreshTokenFactory.Hash(refreshToken);
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var currentToken = await FindRefreshTokenForUpdateAsync(
            tokenHash,
            cancellationToken);

        if (currentToken is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<AuthenticationSession>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        var user = await dbContext.Users
            .SingleAsync(item => item.Id == currentToken.UserId, cancellationToken);
        var now = timeProvider.GetUtcNow();
        await RemoveExpiredTokensAsync(currentToken.UserId, now, cancellationToken);

        if (currentToken.RevokedAt is not null)
        {
            await RevokeFamilyAsync(currentToken.TokenFamilyId, now, ReuseReason, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<AuthenticationSession>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        if (currentToken.ExpiresAt <= now || !user.IsActive)
        {
            currentToken.RevokedAt = now;
            currentToken.RevocationReason = currentToken.ExpiresAt <= now ? "expired" : "user_inactive";
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<AuthenticationSession>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        var newRawToken = RefreshTokenFactory.Generate();
        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = currentToken.UserId,
            TokenFamilyId = currentToken.TokenFamilyId,
            TokenHash = RefreshTokenFactory.Hash(newRawToken),
            CreatedAt = now,
            ExpiresAt = currentToken.ExpiresAt
        };

        currentToken.RevokedAt = now;
        currentToken.RevocationReason = RotatedReason;
        currentToken.ReplacedByTokenId = replacement.Id;

        dbContext.RefreshTokens.Add(replacement);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<AuthenticationSession>.Success(
            BuildSession(user, currentToken.TokenFamilyId, currentToken.ExpiresAt, newRawToken));
    }

    public async Task<Result> LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Success();
        }

        var tokenHash = RefreshTokenFactory.Hash(refreshToken);
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var token = await FindRefreshTokenForUpdateAsync(tokenHash, cancellationToken);

        if (token is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Success();
        }

        var now = timeProvider.GetUtcNow();
        await RevokeFamilyAsync(token.TokenFamilyId, now, LogoutReason, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<AuthenticatedUser>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .Where(item => item.Id == userId && item.IsActive)
            .Select(item => new AuthenticatedUser(item.Id, item.Name, item.Email!))
            .SingleOrDefaultAsync(cancellationToken);

        return user is null
            ? Result<AuthenticatedUser>.Failure(AuthenticationErrors.UserNotFound)
            : Result<AuthenticatedUser>.Success(user);
    }

    private async Task<AuthenticationSession> CreateSessionAsync(
        ApplicationUser user,
        Guid familyId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rawToken = RefreshTokenFactory.Generate();
        var expiresAt = now.AddDays(_options.RefreshTokenDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenFamilyId = familyId,
            TokenHash = RefreshTokenFactory.Hash(rawToken),
            CreatedAt = now,
            ExpiresAt = expiresAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return BuildSession(user, familyId, expiresAt, rawToken);
    }

    private AuthenticationSession BuildSession(
        ApplicationUser user,
        Guid familyId,
        DateTimeOffset sessionExpiresAt,
        string rawRefreshToken)
    {
        var authenticatedUser = new AuthenticatedUser(user.Id, user.Name, user.Email!);
        var accessToken = jwtTokenGenerator.Generate(authenticatedUser, familyId);

        return new AuthenticationSession(
            accessToken.Value,
            accessToken.ExpiresAt,
            sessionExpiresAt,
            authenticatedUser,
            rawRefreshToken);
    }

    private Task<RefreshToken?> FindRefreshTokenForUpdateAsync(
        byte[] tokenHash,
        CancellationToken cancellationToken) =>
        dbContext.RefreshTokens
            .FromSqlInterpolated(
                $"SELECT * FROM refresh_tokens WHERE token_hash = {tokenHash} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    private async Task RevokeFamilyAsync(
        Guid familyId,
        DateTimeOffset now,
        string reason,
        CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(token =>
                token.TokenFamilyId == familyId &&
                token.RevokedAt == null &&
                token.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
            token.RevocationReason = reason;
        }
    }

    private async Task RemoveExpiredTokensAsync(
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var retentionLimit = now.AddDays(-7);
        var oldTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.ExpiresAt < retentionLimit)
            .ToListAsync(cancellationToken);

        dbContext.RefreshTokens.RemoveRange(oldTokens);
    }

    private static Dictionary<string, string[]> ValidateRegistration(
        string name,
        string email,
        string password)
    {
        var errors = new Dictionary<string, string[]>();

        if (name.Length is < 2 or > 100 ||
            !Regex.IsMatch(name, @"^[\p{L}\p{M}][\p{L}\p{M} '\-]*$"))
        {
            errors["name"] = ["Name must have 2 to 100 valid characters."];
        }

        if (email.Length > 254 || !new EmailAddressAttribute().IsValid(email))
        {
            errors["email"] = ["Enter a valid email address."];
        }

        if (password.Length is < 8 or > 128)
        {
            errors["password"] = ["Password must have 8 to 128 characters."];
        }

        return errors;
    }

    private static Result<AuthenticationSession> MapCreationFailure(IdentityResult identityResult)
    {
        if (identityResult.Errors.Any(error =>
                error.Code is "DuplicateEmail" or "DuplicateUserName"))
        {
            return Result<AuthenticationSession>.Failure(
                AuthenticationErrors.EmailAlreadyRegistered);
        }

        var errors = identityResult.Errors
            .GroupBy(error => error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase)
                ? "password"
                : error.Code.Contains("Email", StringComparison.OrdinalIgnoreCase)
                    ? "email"
                    : "request")
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).Distinct().ToArray());

        return Result<AuthenticationSession>.Failure(
            AuthenticationErrors.InvalidRegistration(errors));
    }
}
