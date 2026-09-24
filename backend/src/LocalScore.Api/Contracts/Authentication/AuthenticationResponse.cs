using LocalScore.Application.Authentication;

namespace LocalScore.Api.Contracts.Authentication;

public sealed record UserResponse(Guid Id, string Name, string Email)
{
    public static UserResponse From(AuthenticatedUser user) =>
        new(user.Id, user.Name, user.Email);
}

public sealed record AuthenticationResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset SessionExpiresAt,
    UserResponse User)
{
    public static AuthenticationResponse From(AuthenticationSession session) =>
        new(
            session.AccessToken,
            session.AccessTokenExpiresAt,
            session.SessionExpiresAt,
            UserResponse.From(session.User));
}
