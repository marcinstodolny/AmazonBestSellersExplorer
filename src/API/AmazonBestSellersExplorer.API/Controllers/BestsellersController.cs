using AmazonBestSellersExplorer.API.Common;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.Dtos;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.GetSoftwareBestSellers;
using AmazonBestSellersExplorer.Application.Common;
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
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSoftwareBestSellersQuery(), cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var isServiceUnavailable = result.Errors.Contains(ApplicationMessages.Bestsellers.ServiceUnavailable);

        return result.IsFailed
            ? this.ToProblem(
                result,
                isServiceUnavailable ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status502BadGateway,
                isServiceUnavailable ? ApiProblemTitles.BestsellersUnavailable : ApiProblemTitles.BestsellersRequestFailed)
            : Ok(result.Value);
    }
}
