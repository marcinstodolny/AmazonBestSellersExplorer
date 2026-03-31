using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebApplicationFactory>
{
    public const string Name = "IntegrationTests";
}
