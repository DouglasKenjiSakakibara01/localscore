namespace LocalScore.Application.Championships;

public sealed record ChampionshipStatusModel(int Code, string Description);

public sealed record ChampionshipModel(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    ChampionshipStatusModel Status,
    bool CanChangeSlug,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateChampionshipCommand(
    string Name,
    string? Slug,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record UpdateChampionshipCommand(
    string Name,
    string Slug,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record PublicChampionshipModel(
    string Name,
    string Slug,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    ChampionshipStatusModel Status,
    IReadOnlyList<PublicTeamModel> Teams);

public sealed record PublicTeamModel(
    Guid Id,
    string Name,
    string? ShortName,
    string? Acronym,
    string? City,
    string? PrimaryColor,
    string? SecondaryColor);
