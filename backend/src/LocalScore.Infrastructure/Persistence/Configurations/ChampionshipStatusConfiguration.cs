using LocalScore.Domain.Championships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalScore.Infrastructure.Persistence.Configurations;

internal sealed class ChampionshipStatusConfiguration : IEntityTypeConfiguration<ChampionshipStatus>
{
    public void Configure(EntityTypeBuilder<ChampionshipStatus> builder)
    {
        builder.ToTable("championship_statuses");
        builder.HasKey(status => status.Code).HasName("pk_championship_statuses");

        builder.Property(status => status.Code).HasColumnName("code").HasConversion<int>().ValueGeneratedNever();
        builder.Property(status => status.Description).HasColumnName("description").HasMaxLength(50).IsRequired();

        builder.HasData(
            new { Code = ChampionshipStatusCode.Draft, Description = "Rascunho" },
            new { Code = ChampionshipStatusCode.Published, Description = "Publicado" },
            new { Code = ChampionshipStatusCode.Finished, Description = "Finalizado" });
    }
}
