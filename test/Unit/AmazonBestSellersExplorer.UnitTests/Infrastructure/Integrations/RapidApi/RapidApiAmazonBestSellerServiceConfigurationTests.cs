using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Infrastructure.Integrations.RapidApi;
using Microsoft.Extensions.Options;
using Xunit;

namespace AmazonBestSellersExplorer.UnitTests.Infrastructure.Integrations.RapidApi;

public sealed class RapidApiAmazonBestSellerServiceConfigurationTests
{
    [Fact]
    public async Task GetSoftwareBestSellersAsync_ShouldThrowNeutralConfigurationException_WhenRapidApiOptionsAreInvalid()
    {
        var service = new RapidApiAmazonBestSellerService(
            new HttpClient(new UnusedHttpMessageHandler()),
            Options.Create(new RapidApiOptions()));

        await Assert.ThrowsAsync<AmazonBestSellerServiceConfigurationException>(
            () => service.GetSoftwareBestSellersAsync(CancellationToken.None));
    }

    private sealed class UnusedHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("HTTP should not be called when configuration is invalid.");
        }
    }
}
