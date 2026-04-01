namespace AmazonBestSellersExplorer.Application.Features.Auth.Contracts;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc);
