using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmazonBestSellersExplorer.IntegrationTests.Infrastructure;

internal sealed record RegisterUserRequest(string Username, string Password);

internal sealed record LoginUserRequest(string Username, string Password);

internal sealed record AuthResponseDto(string AccessToken, DateTime ExpiresAtUtc);

internal sealed record AddFavoriteProductRequest(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl);

internal sealed record FavoriteProductResponse(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl);

internal sealed class ProblemDetailsDto
{
    public int? Status { get; init; }

    public string? Title { get; init; }

    public string? Detail { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extensions { get; init; }

    public IReadOnlyList<string> Errors =>
        Extensions is not null &&
        Extensions.TryGetValue("errors", out var errors) &&
        errors.ValueKind == JsonValueKind.Array
            ? errors.EnumerateArray()
                .Where(error => error.ValueKind == JsonValueKind.String)
                .Select(error => error.GetString())
                .OfType<string>()
                .ToArray()
            : [];
}
