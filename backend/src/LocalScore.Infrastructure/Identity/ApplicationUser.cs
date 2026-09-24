using Microsoft.AspNetCore.Identity;

namespace LocalScore.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public required string Name { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; } = [];
}
