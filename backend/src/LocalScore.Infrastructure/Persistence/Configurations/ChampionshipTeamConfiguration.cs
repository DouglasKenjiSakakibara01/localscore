using LocalScore.Domain.Championships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalScore.Infrastructure.Persistence.Configurations;

internal sealed class ChampionshipTeamConfiguration : IEntityTypeConfiguration<ChampionshipTeam>
{
    public void Configure(EntityTypeBuilder<ChampionshipTeam> builder)
    {
        builder.ToTable("championship_teams");
        builder.HasKey(participation => participation.Id).HasName("pk_championship_teams");

        builder.Property(participation => participation.Id).HasColumnName("id");
        builder.Property(participation => participation.ChampionshipId).HasColumnName("championship_id");
        builder.Property(participation => participation.TeamId).HasColumnName("team_id");
        builder.Property(participation => participation.RegisteredAt).HasColumnName("registered_at");
        builder.Property(participation => participation.RemovedAt).HasColumnName("removed_at");
        builder.Ignore(participation => participation.IsActive);

        builder.HasIndex(participation => participation.ChampionshipId)
            .HasDatabaseName("ix_championship_teams_championship_id");
        builder.HasIndex(participation => participation.TeamId)
            .HasDatabaseName("ix_championship_teams_team_id");
        builder.HasIndex(participation => new { participation.ChampionshipId, participation.TeamId })
            .IsUnique()
            .HasDatabaseName("ux_championship_teams_championship_id_team_id");

        builder.HasOne(participation => participation.Championship)
            .WithMany(championship => championship.Teams)
            .HasForeignKey(participation => participation.ChampionshipId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_championship_teams_championships_championship_id");

        builder.HasOne(participation => participation.Team)
            .WithMany(team => team.Championships)
            .HasForeignKey(participation => participation.TeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_championship_teams_teams_team_id");
    }
}
