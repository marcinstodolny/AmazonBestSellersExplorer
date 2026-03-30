namespace AmazonBestSellersExplorer.Application.Features.Auth.Dtos;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc);
