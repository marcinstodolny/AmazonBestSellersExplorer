namespace AmazonBestSellersExplorer.Application.Features.Bestsellers.Dtos;

public sealed record BestsellerProductDto(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl);
