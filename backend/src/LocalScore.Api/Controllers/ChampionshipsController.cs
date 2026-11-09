using LocalScore.Api.Authentication;
using LocalScore.Api.Contracts.Championships;
using LocalScore.Api.Errors;
using LocalScore.Application.Championships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalScore.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/championships")]
public sealed class ChampionshipsController(IChampionshipService championshipService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ChampionshipResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var championships = await championshipService.ListAsync(userId, cancellationToken);
        return Ok(championships.Select(ChampionshipResponse.From).ToArray());
    }

    [HttpGet("{championshipId:guid}")]
    [ProducesResponseType<ChampionshipResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid championshipId, CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await championshipService.GetAsync(userId, championshipId, cancellationToken);
        return result.IsSuccess
            ? Ok(ChampionshipResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    [HttpPost]
    [ProducesResponseType<ChampionshipResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        CreateChampionshipRequest request,
        CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await championshipService.CreateAsync(
            userId,
            new CreateChampionshipCommand(
                request.Name,
                request.Slug,
                request.Description,
                request.StartDate,
                request.EndDate),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { championshipId = result.Value.Id }, ChampionshipResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    [HttpPut("{championshipId:guid}")]
    [ProducesResponseType<ChampionshipResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid championshipId,
        UpdateChampionshipRequest request,
        CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await championshipService.UpdateAsync(
            userId,
            championshipId,
            new UpdateChampionshipCommand(
                request.Name,
                request.Slug,
                request.Description,
                request.StartDate,
                request.EndDate),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ChampionshipResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    [HttpDelete("{championshipId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid championshipId, CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await championshipService.DeleteAsync(userId, championshipId, cancellationToken);
        return result.IsSuccess ? NoContent() : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }

    [HttpPost("{championshipId:guid}/publish")]
    public Task<IActionResult> Publish(Guid championshipId, CancellationToken cancellationToken) =>
        ChangeStatus(championshipId, championshipService.PublishAsync, cancellationToken);

    [HttpPost("{championshipId:guid}/return-to-draft")]
    public Task<IActionResult> ReturnToDraft(Guid championshipId, CancellationToken cancellationToken) =>
        ChangeStatus(championshipId, championshipService.ReturnToDraftAsync, cancellationToken);

    [HttpPost("{championshipId:guid}/finish")]
    public Task<IActionResult> Finish(Guid championshipId, CancellationToken cancellationToken) =>
        ChangeStatus(championshipId, championshipService.FinishAsync, cancellationToken);

    [HttpPost("{championshipId:guid}/reopen")]
    public Task<IActionResult> Reopen(Guid championshipId, CancellationToken cancellationToken) =>
        ChangeStatus(championshipId, championshipService.ReopenAsync, cancellationToken);

    private async Task<IActionResult> ChangeStatus(
        Guid championshipId,
        Func<Guid, Guid, CancellationToken, Task<Application.Common.Result<ChampionshipModel>>> operation,
        CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetId(User, out var userId))
        {
            return ApiProblemDetailsFactory.FromError(HttpContext, CurrentUser.Invalid);
        }

        var result = await operation(userId, championshipId, cancellationToken);
        return result.IsSuccess
            ? Ok(ChampionshipResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }
}
