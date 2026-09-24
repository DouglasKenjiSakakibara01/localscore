namespace LocalScore.Application.Authentication;

public sealed record RegisterCommand(string Name, string Email, string Password);

public sealed record LoginCommand(string Email, string Password);

public sealed record AuthenticatedUser(Guid Id, string Name, string Email);

public sealed record AuthenticationSession(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset SessionExpiresAt,
    AuthenticatedUser User,
    string RefreshToken);
