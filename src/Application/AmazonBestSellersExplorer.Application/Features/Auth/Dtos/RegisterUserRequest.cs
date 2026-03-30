namespace AmazonBestSellersExplorer.Application.Features.Auth.Dtos;

public sealed record RegisterUserRequest(
    string Username,
    string Password);
