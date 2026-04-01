using AmazonBestSellersExplorer.API.Common;
using AmazonBestSellersExplorer.Application.Features.Favorites.AddFavoriteProduct;
using AmazonBestSellersExplorer.Application.Features.Favorites.Contracts;
using AmazonBestSellersExplorer.Application.Features.Favorites.GetFavoriteProducts;
using AmazonBestSellersExplorer.Application.Features.Favorites.RemoveFavoriteProduct;
using AmazonBestSellersExplorer.API.Contracts.Favorites;
using AmazonBestSellersExplorer.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonBestSellersExplorer.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class FavoritesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FavoriteProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFavoriteProductsQuery(), cancellationToken);

        return result.IsFailed
            ? this.ToProblem(result, StatusCodes.Status400BadRequest, ApiProblemTitles.FavoritesRequestFailed)
            : Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add(
        [FromBody] AddFavoriteProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AddFavoriteProductCommand(
                request.AmazonProductId,
                request.Title,
                request.Price,
                request.Rating,
                request.ProductUrl,
                request.ImageUrl),
            cancellationToken);

        return result.IsFailed
            ? this.ToProblem(result, StatusCodes.Status400BadRequest, ApiProblemTitles.FavoritesRequestFailed)
            : Ok();
    }

    [HttpDelete("{amazonProductId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove(
        [FromRoute] string amazonProductId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RemoveFavoriteProductCommand(amazonProductId),
            cancellationToken);

        return result.IsFailed
            ? this.ToProblem(result, StatusCodes.Status400BadRequest, ApiProblemTitles.FavoritesRequestFailed)
            : NoContent();
    }
}
