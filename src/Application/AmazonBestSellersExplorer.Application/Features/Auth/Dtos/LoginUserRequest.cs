namespace AmazonBestSellersExplorer.Application.Features.Auth.Dtos;

public sealed record LoginUserRequest(
    string Username,
    string Password);
