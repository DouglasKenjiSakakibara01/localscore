using LocalScore.Application.Championships;

namespace LocalScore.Api.Contracts.Championships;

public sealed record ChampionshipStatusResponse(int Code, string Description)
{
    public static ChampionshipStatusResponse From(ChampionshipStatusModel status) =>
        new(status.Code, status.Description);
}

public sealed record ChampionshipResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    ChampionshipStatusResponse Status,
    bool CanChangeSlug,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static ChampionshipResponse From(ChampionshipModel championship) =>
        new(
            championship.Id,
            championship.Name,
            championship.Slug,
            championship.Description,
            championship.StartDate,
            championship.EndDate,
            ChampionshipStatusResponse.From(championship.Status),
            championship.CanChangeSlug,
            championship.CreatedAt,
            championship.UpdatedAt);
}

public sealed record PublicTeamResponse(
    Guid Id,
    string Name,
    string? ShortName,
    string? Acronym,
    string? City,
    string? PrimaryColor,
    string? SecondaryColor)
{
    public static PublicTeamResponse From(PublicTeamModel team) =>
        new(team.Id, team.Name, team.ShortName, team.Acronym, team.City, team.PrimaryColor, team.SecondaryColor);
}

public sealed record PublicChampionshipResponse(
    string Name,
    string Slug,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    ChampionshipStatusResponse Status,
    IReadOnlyList<PublicTeamResponse> Teams)
{
    public static PublicChampionshipResponse From(PublicChampionshipModel championship) =>
        new(
            championship.Name,
            championship.Slug,
            championship.Description,
            championship.StartDate,
            championship.EndDate,
            ChampionshipStatusResponse.From(championship.Status),
            championship.Teams.Select(PublicTeamResponse.From).ToArray());
}
