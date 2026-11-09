namespace LocalScore.Application.Teams;

public sealed record TeamModel(
    Guid Id,
    string Name,
    string? ShortName,
    string? Acronym,
    string? City,
    string? PrimaryColor,
    string? SecondaryColor,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SaveTeamCommand(
    string Name,
    string? ShortName,
    string? Acronym,
    string? City,
    string? PrimaryColor,
    string? SecondaryColor);

public sealed record ChampionshipTeamModel(
    Guid Id,
    TeamModel Team,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? RemovedAt)
{
    public bool IsActive => RemovedAt is null;
}

public sealed record ParticipationResult(
    ChampionshipTeamModel Participation,
    bool WasCreated);
