using LocalScore.Application.Common;

namespace LocalScore.Application.Authentication;

public static class AuthenticationErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("auth.invalid_credentials", "Invalid email or password.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("auth.invalid_refresh_token", "The session could not be renewed.");

    public static readonly Error UserNotFound =
        Error.NotFound("auth.user_not_found", "The authenticated user was not found.");

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("auth.email_already_registered", "This email address is already registered.");

    public static Error InvalidRegistration(IReadOnlyDictionary<string, string[]> errors) =>
        Error.Validation("auth.registration_invalid", "The registration data is invalid.", errors);
}
