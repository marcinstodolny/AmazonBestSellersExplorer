using System.Net.Http.Json;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Infrastructure;

internal static class HttpResponseMessageExtensions
{
    public static async Task<ProblemDetailsDto> ReadProblemDetailsAsync(this HttpResponseMessage response)
    {
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();

        Assert.NotNull(problemDetails);

        return problemDetails;
    }
}
