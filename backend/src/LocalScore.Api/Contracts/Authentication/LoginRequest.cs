using System.ComponentModel.DataAnnotations;

namespace LocalScore.Api.Contracts.Authentication;

public sealed class LoginRequest
{
    [Required]
    [StringLength(254)]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}
