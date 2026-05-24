namespace AmazonBestSellersExplorer.API.Contracts.Auth;

public sealed record LoginUserRequest(
    string Username,
    string Password);
