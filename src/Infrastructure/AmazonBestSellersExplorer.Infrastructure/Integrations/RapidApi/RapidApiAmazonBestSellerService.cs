using System.Globalization;
using System.Net.Http.Json;
using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.Dtos;
using AmazonBestSellersExplorer.Infrastructure.Integrations.RapidApi.Models;

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

        var priceParts = normalizedPrice.Split(['.', ',']);

        if (priceParts.Length > 1
            && priceParts.Skip(1).All(static part => part.Length == 3))
        {
            normalizedPrice = string.Concat(priceParts);
        }
        else if (normalizedPrice.Any(static character => character is '.' or ','))
        {
            var lastSeparatorIndex = normalizedPrice.LastIndexOfAny(['.', ',']);
            normalizedPrice = normalizedPrice[..lastSeparatorIndex].Replace(".", string.Empty).Replace(",", string.Empty)
                + "."
                + normalizedPrice[(lastSeparatorIndex + 1)..];
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
}
