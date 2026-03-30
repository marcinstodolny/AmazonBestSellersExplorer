namespace AmazonBestSellersExplorer.Application.Features.Auth.RegisterUser;

public sealed record RegisterUserCommand(
    string Username,
    string Password);
