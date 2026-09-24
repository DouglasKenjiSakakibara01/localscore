namespace LocalScore.Infrastructure.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TokenFamilyId { get; set; }

    public required byte[] TokenHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? RevocationReason { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public RefreshToken? ReplacedByToken { get; set; }
}
