namespace LocalScore.Domain.Championships;

public sealed class ChampionshipTeam
{
    private ChampionshipTeam()
    {
    }

    private ChampionshipTeam(
        Guid id,
        Guid championshipId,
        Guid teamId,
        DateTimeOffset registeredAt)
    {
        Id = id;
        ChampionshipId = championshipId;
        TeamId = teamId;
        RegisteredAt = registeredAt;
    }

    public Guid Id { get; private set; }

    public Guid ChampionshipId { get; private set; }

    public Guid TeamId { get; private set; }

    public DateTimeOffset RegisteredAt { get; private set; }

    public DateTimeOffset? RemovedAt { get; private set; }

    public Championship Championship { get; private set; } = null!;

    public Team Team { get; private set; } = null!;

    public bool IsActive => RemovedAt is null;

    public static ChampionshipTeam Create(
        Guid championshipId,
        Guid teamId,
        DateTimeOffset now) =>
        new(Guid.NewGuid(), championshipId, teamId, now);

    public void Activate(DateTimeOffset now)
    {
        RegisteredAt = now;
        RemovedAt = null;
    }

    public void Remove(DateTimeOffset now)
    {
        RemovedAt ??= now;
    }
}
