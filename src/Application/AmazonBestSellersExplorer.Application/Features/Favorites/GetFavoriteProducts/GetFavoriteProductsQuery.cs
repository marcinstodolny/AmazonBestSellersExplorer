using AmazonBestSellersExplorer.Application.Abstractions.Authentication;
using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Features.Favorites.Dtos;
using AmazonBestSellersExplorer.Domain.Base;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Favorites.GetFavoriteProducts;

public sealed record GetFavoriteProductsQuery
    : IRequest<Result<IReadOnlyList<FavoriteProductDto>>>;

public sealed class GetFavoriteProductsQueryHandler(
    ICurrentUserContext currentUserContext,
    IFavoriteProductRepository favoriteProductRepository)
    : IRequestHandler<GetFavoriteProductsQuery, Result<IReadOnlyList<FavoriteProductDto>>>
{
    public async Task<Result<IReadOnlyList<FavoriteProductDto>>> Handle(
        GetFavoriteProductsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserContext.IsAuthenticated || currentUserContext.UserId is null)
        {
            return Result.Fail<IReadOnlyList<FavoriteProductDto>>("User is not authenticated.");
        }

        var favoriteProducts = await favoriteProductRepository.GetByUserIdAsync(
            currentUserContext.UserId.Value,
            cancellationToken);

        var favoriteProductDtos = favoriteProducts
            .Select(static favoriteProduct => new FavoriteProductDto(
                favoriteProduct.AmazonProductId,
                favoriteProduct.Title,
                favoriteProduct.Price,
                favoriteProduct.Rating,
                favoriteProduct.ProductUrl,
                favoriteProduct.ImageUrl))
            .ToArray();

        return Result.Success<IReadOnlyList<FavoriteProductDto>>(favoriteProductDtos);
    }
}
