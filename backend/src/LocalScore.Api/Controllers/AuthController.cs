using System.IdentityModel.Tokens.Jwt;
using LocalScore.Api.Contracts.Authentication;
using LocalScore.Api.Errors;
using LocalScore.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LocalScore.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    IAuthenticationService authenticationService) : ControllerBase
{
    private const string RefreshCookieName = "LocalScore.RefreshToken";

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("register")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.RegisterAsync(
            new RegisterCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
        }

        SetRefreshCookie(result.Value);
        return StatusCode(
            StatusCodes.Status201Created,
            AuthenticationResponse.From(result.Value));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
        }

        SetRefreshCookie(result.Value);
        return Ok(AuthenticationResponse.From(result.Value));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("refresh")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);
        var result = await authenticationService.RefreshAsync(
            refreshToken ?? string.Empty,
            cancellationToken);

        if (result.IsFailure)
        {
            DeleteRefreshCookie();
            return ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
        }

        SetRefreshCookie(result.Value);
        return Ok(AuthenticationResponse.From(result.Value));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);
        await authenticationService.LogoutAsync(refreshToken, cancellationToken);
        DeleteRefreshCookie();

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(subject, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(
                HttpContext,
                AuthenticationErrors.InvalidCredentials);
        }

        var result = await authenticationService.GetCurrentUserAsync(userId, cancellationToken);

        return result.IsSuccess
            ? Ok(UserResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    private void SetRefreshCookie(AuthenticationSession session)
    {
        var maxAge = session.SessionExpiresAt - DateTimeOffset.UtcNow;

        Response.Cookies.Append(
            RefreshCookieName,
            session.RefreshToken,
            CreateCookieOptions(maxAge > TimeSpan.Zero ? maxAge : TimeSpan.Zero));
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(
            RefreshCookieName,
            CreateCookieOptions(TimeSpan.Zero));
    }

    private static CookieOptions CreateCookieOptions(TimeSpan maxAge) =>
        new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/auth",
            MaxAge = maxAge,
            IsEssential = true
        };
}
