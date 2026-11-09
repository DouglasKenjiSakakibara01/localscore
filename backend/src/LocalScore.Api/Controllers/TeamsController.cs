using LocalScore.Api.Authentication;
using LocalScore.Api.Contracts.Teams;
using LocalScore.Api.Errors;
using LocalScore.Application.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalScore.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/teams")]
public sealed class TeamsController(ITeamService teamService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var teams = await teamService.ListAsync(userId, cancellationToken);
        return Ok(teams.Select(TeamResponse.From).ToArray());
    }

    [HttpGet("{teamId:guid}")]
    public async Task<IActionResult> Get(Guid teamId, CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await teamService.GetAsync(userId, teamId, cancellationToken);
        return result.IsSuccess
            ? Ok(TeamResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaveTeamRequest request, CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await teamService.CreateAsync(userId, ToCommand(request), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { teamId = result.Value.Id }, TeamResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    [HttpPut("{teamId:guid}")]
    public async Task<IActionResult> Update(
        Guid teamId,
        SaveTeamRequest request,
        CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await teamService.UpdateAsync(userId, teamId, ToCommand(request), cancellationToken);
        return result.IsSuccess
            ? Ok(TeamResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    [HttpDelete("{teamId:guid}")]
    public async Task<IActionResult> Delete(Guid teamId, CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await teamService.DeleteAsync(userId, teamId, cancellationToken);
        return result.IsSuccess ? NoContent() : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    private static SaveTeamCommand ToCommand(SaveTeamRequest request) =>
        new(
            request.Name,
            request.ShortName,
            request.Acronym,
            request.City,
            request.PrimaryColor,
            request.SecondaryColor);
}
