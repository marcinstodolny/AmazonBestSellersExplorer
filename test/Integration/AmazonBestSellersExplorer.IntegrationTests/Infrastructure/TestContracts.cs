namespace AmazonBestSellersExplorer.IntegrationTests.Infrastructure;

internal sealed record RegisterUserRequest(string Username, string Password);

internal sealed record LoginUserRequest(string Username, string Password);

internal sealed record AuthResponseDto(string AccessToken, DateTime ExpiresAtUtc);

internal sealed record AddFavoriteProductRequest(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl);

internal sealed record FavoriteProductDto(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl);
