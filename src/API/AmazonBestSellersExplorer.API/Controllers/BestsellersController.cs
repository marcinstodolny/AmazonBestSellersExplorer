using AmazonBestSellersExplorer.API.Common;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.Dtos;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.GetSoftwareBestSellers;
using AmazonBestSellersExplorer.API.Extensions;
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

        return result.IsFailed
            ? this.ToProblem(result, StatusCodes.Status502BadGateway, ApiProblemTitles.BestsellersRequestFailed)
            : Ok(result.Value);
    }
}
