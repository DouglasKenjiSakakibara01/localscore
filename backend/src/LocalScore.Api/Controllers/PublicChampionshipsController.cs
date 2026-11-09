using LocalScore.Api.Contracts.Championships;
using LocalScore.Api.Errors;
using LocalScore.Application.Championships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalScore.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/championships")]
public sealed class PublicChampionshipsController(IChampionshipService championshipService) : ControllerBase
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> Get(string slug, CancellationToken cancellationToken)
    {
        var result = await championshipService.GetPublicAsync(slug, cancellationToken);
        return result.IsSuccess
            ? Ok(PublicChampionshipResponse.From(result.Value))
            : ApiProblemDetailsFactory.FromError(HttpContext, result.Error);
    }
}
