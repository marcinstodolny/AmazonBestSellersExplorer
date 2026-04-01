namespace AmazonBestSellersExplorer.Application.Features.Bestsellers.Contracts;

public sealed record BestsellerProductResponse(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl);
