using System.ComponentModel.DataAnnotations;

namespace LocalScore.Api.Contracts.Authentication;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(
        @"^[\p{L}\p{M}][\p{L}\p{M} '\-]*$",
        ErrorMessage = "Name contains invalid characters.")]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(254)]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one lowercase letter and one number.")]
    public string Password { get; init; } = string.Empty;
}
