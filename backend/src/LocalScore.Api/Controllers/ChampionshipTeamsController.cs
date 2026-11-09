using LocalScore.Api.Authentication;
using LocalScore.Api.Contracts.Teams;
using LocalScore.Api.Errors;
using LocalScore.Application.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalScore.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/championships/{championshipId:guid}/teams")]
public sealed class ChampionshipTeamsController(ITeamService teamService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid championshipId, CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await teamService.ListParticipantsAsync(userId, championshipId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value.Select(ChampionshipTeamResponse.From).ToArray())
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    [HttpPut("{teamId:guid}")]
    public async Task<IActionResult> Activate(
        Guid championshipId,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await teamService.ActivateParticipationAsync(
            userId,
            championshipId,
            teamId,
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
        }

        var response = ChampionshipTeamResponse.From(result.Value.Participation);
        return result.Value.WasCreated
            ? StatusCode(StatusCodes.Status201Created, response)
            : Ok(response);
    }

    [HttpDelete("{teamId:guid}")]
    public async Task<IActionResult> Remove(
        Guid championshipId,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await teamService.RemoveParticipationAsync(
            userId,
            championshipId,
            teamId,
            cancellationToken);

        return result.IsSuccess ? NoContent() : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }
}
