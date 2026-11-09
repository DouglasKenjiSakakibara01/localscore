namespace LocalScore.Domain.Championships;

public sealed class ChampionshipStatus
{
    private ChampionshipStatus()
    {
    }

    public ChampionshipStatusCode Code { get; private set; }

    public string Description { get; private set; } = string.Empty;
}
