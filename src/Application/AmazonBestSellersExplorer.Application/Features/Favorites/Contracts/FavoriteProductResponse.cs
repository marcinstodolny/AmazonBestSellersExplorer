namespace AmazonBestSellersExplorer.Application.Features.Favorites.Contracts;

public sealed record FavoriteProductResponse(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl);
