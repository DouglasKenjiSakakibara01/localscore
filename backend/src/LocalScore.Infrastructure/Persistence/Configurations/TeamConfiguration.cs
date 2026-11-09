using LocalScore.Domain.Championships;
using LocalScore.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalScore.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");
        builder.HasKey(team => team.Id).HasName("pk_teams");

        builder.Property(team => team.Id).HasColumnName("id");
        builder.Property(team => team.OwnerUserId).HasColumnName("owner_user_id");
        builder.Property(team => team.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(team => team.NormalizedName).HasColumnName("normalized_name").HasMaxLength(100).IsRequired();
        builder.Property(team => team.ShortName).HasColumnName("short_name").HasMaxLength(30);
        builder.Property(team => team.Acronym).HasColumnName("acronym").HasMaxLength(10);
        builder.Property(team => team.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(team => team.PrimaryColor).HasColumnName("primary_color").HasMaxLength(7);
        builder.Property(team => team.SecondaryColor).HasColumnName("secondary_color").HasMaxLength(7);
        builder.Property(team => team.CreatedAt).HasColumnName("created_at");
        builder.Property(team => team.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(team => team.OwnerUserId)
            .HasDatabaseName("ix_teams_owner_user_id");
        builder.HasIndex(team => new { team.OwnerUserId, team.NormalizedName })
            .IsUnique()
            .HasDatabaseName("ux_teams_owner_user_id_normalized_name");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(team => team.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_teams_users_owner_user_id");
    }
}
