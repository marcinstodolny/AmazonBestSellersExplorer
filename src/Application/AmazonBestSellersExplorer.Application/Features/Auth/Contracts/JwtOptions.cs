namespace AmazonBestSellersExplorer.Application.Features.Auth.Contracts;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; }
}
