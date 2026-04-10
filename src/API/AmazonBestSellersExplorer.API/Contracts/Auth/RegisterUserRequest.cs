namespace AmazonBestSellersExplorer.API.Contracts.Auth;

public sealed record RegisterUserRequest(
    string Username,
    string Password);
