using System.Net;
using System.Net.Http.Json;
using AmazonBestSellersExplorer.Infrastructure.Integrations.RapidApi;
using Xunit;

namespace AmazonBestSellersExplorer.UnitTests.Infrastructure.Integrations.RapidApi;

public sealed class RapidApiAmazonBestSellerServiceTests
{
    [Fact]
    public async Task GetSoftwareBestSellersAsync_ShouldSendExpectedRapidApiRequest()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    data = new
                    {
                        best_sellers = Array.Empty<object>()
                    }
                })
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var service = new RapidApiAmazonBestSellerService(httpClient);

        await service.GetSoftwareBestSellersAsync(CancellationToken.None);

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest!.Method);
        Assert.Equal("/best-sellers", capturedRequest.RequestUri!.AbsolutePath);

        var query = ParseQuery(capturedRequest.RequestUri.Query);
        Assert.Equal("software", query["category"]);
        Assert.Equal("PL", query["country"]);
        Assert.Equal("BEST_SELLERS", query["type"]);
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        return query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(static part => part.Split('=', 2))
            .ToDictionary(
                static parts => Uri.UnescapeDataString(parts[0]),
                static parts => parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty,
                StringComparer.Ordinal);
    }

    private sealed class CapturingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(responseFactory(request));
        }
    }
}
