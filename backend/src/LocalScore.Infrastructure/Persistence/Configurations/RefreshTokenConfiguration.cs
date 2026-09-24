using LocalScore.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalScore.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(token => token.Id).HasName("pk_refresh_tokens");

        builder.Property(token => token.Id).HasColumnName("id");
        builder.Property(token => token.UserId).HasColumnName("user_id");
        builder.Property(token => token.TokenFamilyId).HasColumnName("token_family_id");
        builder.Property(token => token.TokenHash).HasColumnName("token_hash").HasColumnType("bytea").IsRequired();
        builder.Property(token => token.CreatedAt).HasColumnName("created_at");
        builder.Property(token => token.ExpiresAt).HasColumnName("expires_at");
        builder.Property(token => token.RevokedAt).HasColumnName("revoked_at");
        builder.Property(token => token.RevocationReason).HasColumnName("revocation_reason").HasMaxLength(50);
        builder.Property(token => token.ReplacedByTokenId).HasColumnName("replaced_by_token_id");

        builder.HasIndex(token => token.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_refresh_tokens_token_hash");

        builder.HasIndex(token => token.UserId).HasDatabaseName("ix_refresh_tokens_user_id");
        builder.HasIndex(token => token.TokenFamilyId).HasDatabaseName("ix_refresh_tokens_token_family_id");
        builder.HasIndex(token => token.ExpiresAt).HasDatabaseName("ix_refresh_tokens_expires_at");
        builder.HasIndex(token => token.ReplacedByTokenId).HasDatabaseName("ix_refresh_tokens_replaced_by_token_id");

        builder.HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_refresh_tokens_users_user_id");

        builder.HasOne(token => token.ReplacedByToken)
            .WithMany()
            .HasForeignKey(token => token.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_refresh_tokens_refresh_tokens_replaced_by_token_id");
    }
}
