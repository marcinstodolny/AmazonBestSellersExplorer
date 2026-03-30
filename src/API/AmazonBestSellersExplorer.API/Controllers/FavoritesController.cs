using AmazonBestSellersExplorer.Application.Features.Favorites.AddFavoriteProduct;
using AmazonBestSellersExplorer.Application.Features.Favorites.Dtos;
using AmazonBestSellersExplorer.Application.Features.Favorites.GetFavoriteProducts;
using AmazonBestSellersExplorer.Application.Features.Favorites.RemoveFavoriteProduct;
using AmazonBestSellersExplorer.Domain.Base;
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
    [ProducesResponseType<IReadOnlyList<FavoriteProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFavoriteProductsQuery(), cancellationToken);

        return ToOkOrBadRequest(result);
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

        if (result.IsSuccess)
        {
            return Ok();
        }

        return BadRequest(new { errors = result.Errors });
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

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(new { errors = result.Errors });
    }

    private IActionResult ToOkOrBadRequest(Result<IReadOnlyList<FavoriteProductDto>> result)
    {
        if (result.TryGetValue(out var favoriteProducts))
        {
            return Ok(favoriteProducts);
        }

        return BadRequest(new { errors = result.Errors });
    }
}
