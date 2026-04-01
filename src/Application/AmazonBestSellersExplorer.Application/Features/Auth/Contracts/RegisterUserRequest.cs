namespace AmazonBestSellersExplorer.Application.Features.Auth.Contracts;

public sealed record RegisterUserRequest(
    string Username,
    string Password);
