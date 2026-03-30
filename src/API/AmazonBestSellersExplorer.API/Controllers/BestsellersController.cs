using AmazonBestSellersExplorer.Application.Features.Bestsellers.Dtos;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.GetSoftwareBestSellers;
using AmazonBestSellersExplorer.Domain.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonBestSellersExplorer.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/bestsellers")]
public sealed class BestsellersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BestsellerProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSoftwareBestSellersQuery(), cancellationToken);

        return ToActionResult(result);
    }

    private IActionResult ToActionResult(Result<IReadOnlyList<BestsellerProductDto>> result)
    {
        if (result.TryGetValue(out var products))
        {
            return Ok(products);
        }

        return StatusCode(StatusCodes.Status502BadGateway, new { errors = result.Errors });
    }
}
