namespace AmazonBestSellersExplorer.Infrastructure.Integrations.RapidApi;

public sealed class RapidApiOptions
{
    public const string SectionName = "RapidApi";

    public string BaseUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public string ApiHost { get; init; } = string.Empty;
}
