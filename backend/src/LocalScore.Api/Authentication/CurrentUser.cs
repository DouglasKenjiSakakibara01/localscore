using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LocalScore.Application.Common;

namespace LocalScore.Api.Authentication;

internal static class CurrentUser
{
    public static readonly Error Invalid = Error.Unauthorized(
        "auth.invalid_subject",
        "The authenticated user identifier is invalid.");

    public static bool TryGetId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out userId);
}
