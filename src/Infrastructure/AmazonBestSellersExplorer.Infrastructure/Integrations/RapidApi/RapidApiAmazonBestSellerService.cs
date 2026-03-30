using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.Dtos;

namespace AmazonBestSellersExplorer.Infrastructure.Integrations.RapidApi;

public sealed class RapidApiAmazonBestSellerService(HttpClient httpClient) : IAmazonBestSellerService
{
    private const string BestSellersRequestUri = "best-sellers?category=software&country=PL&type=BEST_SELLERS";

    public async Task<IReadOnlyList<BestsellerProductDto>> GetSoftwareBestSellersAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(BestSellersRequestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RapidApiBestSellersResponse>(cancellationToken);
        var items = payload?.Data?.BestSellers ?? Array.Empty<RapidApiBestSellerProduct>();

        return items
            .Where(static product =>
                !string.IsNullOrWhiteSpace(product.Asin)
                && !string.IsNullOrWhiteSpace(product.ProductTitle)
                && !string.IsNullOrWhiteSpace(product.ProductUrl))
            .Select(static product => new BestsellerProductDto(
                product.Asin!,
                product.ProductTitle!,
                ParsePrice(product.ProductPrice),
                ParseRating(product.ProductStarRating),
                product.ProductUrl!,
                product.ProductPhoto))
            .ToArray();
    }

    private static decimal? ParsePrice(string? rawPrice)
    {
        if (string.IsNullOrWhiteSpace(rawPrice))
        {
            return null;
        }

        var normalizedPrice = new string(rawPrice
            .Where(static character => char.IsDigit(character) || character is ',' or '.')
            .ToArray());

        if (string.IsNullOrWhiteSpace(normalizedPrice))
        {
            return null;
        }

        if (normalizedPrice.Count(static character => character is '.' or ',') > 1)
        {
            var lastSeparatorIndex = normalizedPrice.LastIndexOfAny(['.', ',']);
            normalizedPrice = normalizedPrice[..lastSeparatorIndex].Replace(".", string.Empty).Replace(",", string.Empty)
                + "."
                + normalizedPrice[(lastSeparatorIndex + 1)..];
        }
        else
        {
            normalizedPrice = normalizedPrice.Replace(',', '.');
        }

        return decimal.TryParse(
            normalizedPrice,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out var price)
            ? price
            : null;
    }

    private static double? ParseRating(string? rawRating)
    {
        if (string.IsNullOrWhiteSpace(rawRating))
        {
            return null;
        }

        var normalizedRating = rawRating.Replace(',', '.');

        return double.TryParse(
            normalizedRating,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out var rating)
            ? rating
            : null;
    }

    private sealed class RapidApiBestSellersResponse
    {
        [JsonPropertyName("data")]
        public RapidApiBestSellersData? Data { get; init; }
    }

    private sealed class RapidApiBestSellersData
    {
        [JsonPropertyName("best_sellers")]
        public IReadOnlyList<RapidApiBestSellerProduct>? BestSellers { get; init; }
    }

    private sealed class RapidApiBestSellerProduct
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
}
