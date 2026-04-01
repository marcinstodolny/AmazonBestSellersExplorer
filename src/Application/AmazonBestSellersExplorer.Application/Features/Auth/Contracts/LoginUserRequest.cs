namespace AmazonBestSellersExplorer.Application.Features.Auth.Contracts;

public sealed record LoginUserRequest(
    string Username,
    string Password);
