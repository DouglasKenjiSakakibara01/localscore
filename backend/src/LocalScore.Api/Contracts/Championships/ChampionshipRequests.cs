using System.ComponentModel.DataAnnotations;

namespace LocalScore.Api.Contracts.Championships;

public sealed record CreateChampionshipRequest(
    [Required, StringLength(120, MinimumLength = 3)] string Name,
    [StringLength(100, MinimumLength = 3)] string? Slug,
    [StringLength(1000)] string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record UpdateChampionshipRequest(
    [Required, StringLength(120, MinimumLength = 3)] string Name,
    [Required, StringLength(100, MinimumLength = 3)] string Slug,
    [StringLength(1000)] string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);
