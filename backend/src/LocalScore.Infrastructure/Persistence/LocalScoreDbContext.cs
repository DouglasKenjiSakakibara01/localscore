using LocalScore.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LocalScore.Infrastructure.Persistence;

public sealed class LocalScoreDbContext(
    DbContextOptions<LocalScoreDbContext> options)
    : IdentityUserContext<
        ApplicationUser,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserLogin<Guid>,
        IdentityUserToken<Guid>>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(LocalScoreDbContext).Assembly);

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("user_claims");
            entity.HasKey(claim => claim.Id).HasName("pk_user_claims");
            entity.Property(claim => claim.Id).HasColumnName("id");
            entity.Property(claim => claim.UserId).HasColumnName("user_id");
            entity.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            entity.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
            entity.HasIndex(claim => claim.UserId).HasDatabaseName("ix_user_claims_user_id");
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(claim => claim.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_user_claims_users_user_id");
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("user_logins");
            entity.HasKey(login => new { login.LoginProvider, login.ProviderKey }).HasName("pk_user_logins");
            entity.Property(login => login.LoginProvider).HasColumnName("login_provider");
            entity.Property(login => login.ProviderKey).HasColumnName("provider_key");
            entity.Property(login => login.ProviderDisplayName).HasColumnName("provider_display_name");
            entity.Property(login => login.UserId).HasColumnName("user_id");
            entity.HasIndex(login => login.UserId).HasDatabaseName("ix_user_logins_user_id");
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(login => login.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_user_logins_users_user_id");
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("user_tokens");
            entity.HasKey(token => new { token.UserId, token.LoginProvider, token.Name }).HasName("pk_user_tokens");
            entity.Property(token => token.UserId).HasColumnName("user_id");
            entity.Property(token => token.LoginProvider).HasColumnName("login_provider");
            entity.Property(token => token.Name).HasColumnName("name");
            entity.Property(token => token.Value).HasColumnName("value");
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_user_tokens_users_user_id");
        });
    }
}
