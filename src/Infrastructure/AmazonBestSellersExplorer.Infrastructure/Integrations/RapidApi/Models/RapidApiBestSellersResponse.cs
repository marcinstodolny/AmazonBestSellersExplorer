using System.Text.Json.Serialization;

namespace AmazonBestSellersExplorer.Infrastructure.Integrations.RapidApi.Models;

internal sealed class RapidApiBestSellersResponse
{
    [JsonPropertyName("data")]
    public RapidApiBestSellersData? Data { get; init; }
}

internal sealed class RapidApiBestSellersData
{
    [JsonPropertyName("best_sellers")]
    public IReadOnlyList<RapidApiBestSellerProduct>? BestSellers { get; init; }
}

internal sealed class RapidApiBestSellerProduct
{
    [JsonPropertyName("asin")]
    public string? Asin { get; init; }

    [JsonPropertyName("product_title")]
    public string? ProductTitle { get; init; }

    [JsonPropertyName("product_price")]
    public string? ProductPrice { get; init; }

    [JsonPropertyName("product_star_rating")]
    public string? ProductStarRating { get; init; }

    [JsonPropertyName("product_url")]
    public string? ProductUrl { get; init; }

    [JsonPropertyName("product_photo")]
    public string? ProductPhoto { get; init; }
}
