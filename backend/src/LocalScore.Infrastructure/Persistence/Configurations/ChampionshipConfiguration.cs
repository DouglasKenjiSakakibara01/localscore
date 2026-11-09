using LocalScore.Domain.Championships;
using LocalScore.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalScore.Infrastructure.Persistence.Configurations;

internal sealed class ChampionshipConfiguration : IEntityTypeConfiguration<Championship>
{
    public void Configure(EntityTypeBuilder<Championship> builder)
    {
        builder.ToTable("championships");
        builder.HasKey(championship => championship.Id).HasName("pk_championships");

        builder.Property(championship => championship.Id).HasColumnName("id");
        builder.Property(championship => championship.OwnerUserId).HasColumnName("owner_user_id");
        builder.Property(championship => championship.StatusCode).HasColumnName("status_code").HasConversion<int>();
        builder.Property(championship => championship.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(championship => championship.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.Property(championship => championship.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(championship => championship.StartDate).HasColumnName("start_date").HasColumnType("date");
        builder.Property(championship => championship.EndDate).HasColumnName("end_date").HasColumnType("date");
        builder.Property(championship => championship.PublishedAt).HasColumnName("published_at");
        builder.Property(championship => championship.CreatedAt).HasColumnName("created_at");
        builder.Property(championship => championship.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(championship => championship.OwnerUserId)
            .HasDatabaseName("ix_championships_owner_user_id");
        builder.HasIndex(championship => championship.StatusCode)
            .HasDatabaseName("ix_championships_status_code");
        builder.HasIndex(championship => championship.Slug)
            .IsUnique()
            .HasDatabaseName("ux_championships_slug");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(championship => championship.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_championships_users_owner_user_id");

        builder.HasOne<ChampionshipStatus>()
            .WithMany()
            .HasForeignKey(championship => championship.StatusCode)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_championships_championship_statuses_status_code");
    }
}
