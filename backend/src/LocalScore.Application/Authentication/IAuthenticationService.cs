using LocalScore.Application.Common;

namespace LocalScore.Application.Authentication;

public interface IAuthenticationService
{
    Task<Result<AuthenticationSession>> RegisterAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationSession>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationSession>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticatedUser>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
