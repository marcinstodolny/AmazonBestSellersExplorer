namespace AmazonBestSellersExplorer.Application.Features.Favorites.Dtos;

public sealed record AddFavoriteProductRequest(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl);
