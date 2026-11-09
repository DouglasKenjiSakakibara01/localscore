using System.ComponentModel.DataAnnotations;
using LocalScore.Application.Teams;

namespace LocalScore.Api.Contracts.Teams;

public sealed record SaveTeamRequest(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [StringLength(30, MinimumLength = 2)] string? ShortName,
    [StringLength(10, MinimumLength = 2), RegularExpression("^[A-Za-z0-9]+$")] string? Acronym,
    [StringLength(100, MinimumLength = 2)] string? City,
    [RegularExpression("^#[0-9A-Fa-f]{6}$")] string? PrimaryColor,
    [RegularExpression("^#[0-9A-Fa-f]{6}$")] string? SecondaryColor);

public sealed record TeamResponse(
    Guid Id,
    string Name,
    string? ShortName,
    string? Acronym,
    string? City,
    string? PrimaryColor,
    string? SecondaryColor,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static TeamResponse From(TeamModel team) =>
        new(
            team.Id,
            team.Name,
            team.ShortName,
            team.Acronym,
            team.City,
            team.PrimaryColor,
            team.SecondaryColor,
            team.CreatedAt,
            team.UpdatedAt);
}

public sealed record ChampionshipTeamResponse(
    Guid Id,
    TeamResponse Team,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? RemovedAt,
    bool IsActive)
{
    public static ChampionshipTeamResponse From(ChampionshipTeamModel participation) =>
        new(
            participation.Id,
            TeamResponse.From(participation.Team),
            participation.RegisteredAt,
            participation.RemovedAt,
            participation.IsActive);
}
