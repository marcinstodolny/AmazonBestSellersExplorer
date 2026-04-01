using AmazonBestSellersExplorer.Application.Abstractions.Authentication;
using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Common;
using AmazonBestSellersExplorer.Application.Features.Favorites.Contracts;
using AmazonBestSellersExplorer.Domain.Base;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Favorites.GetFavoriteProducts;

public sealed record GetFavoriteProductsQuery
    : IRequest<Result<IReadOnlyList<FavoriteProductResponse>>>;

public sealed class GetFavoriteProductsQueryHandler(
    ICurrentUserContext currentUserContext,
    IFavoriteProductRepository favoriteProductRepository)
    : IRequestHandler<GetFavoriteProductsQuery, Result<IReadOnlyList<FavoriteProductResponse>>>
{
    public async Task<Result<IReadOnlyList<FavoriteProductResponse>>> Handle(
        GetFavoriteProductsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserContext.IsAuthenticated || currentUserContext.UserId is null)
        {
            return Result.Fail<IReadOnlyList<FavoriteProductResponse>>(ApplicationMessages.Favorites.UserNotAuthenticated);
        }

        var favoriteProducts = await favoriteProductRepository.GetByUserIdAsync(
            currentUserContext.UserId.Value,
            cancellationToken);

        var favoriteProductResponses = favoriteProducts
            .Select(static favoriteProduct => new FavoriteProductResponse(
                favoriteProduct.AmazonProductId,
                favoriteProduct.Title,
                favoriteProduct.Price,
                favoriteProduct.Rating,
                favoriteProduct.ProductUrl,
                favoriteProduct.ImageUrl))
            .ToArray();

        return Result.Success<IReadOnlyList<FavoriteProductResponse>>(favoriteProductResponses);
    }
}
