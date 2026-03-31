using AmazonBestSellersExplorer.API;
using AmazonBestSellersExplorer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AmazonBestSellersExplorer.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestDatabaseName = "AmazonBestSellersExplorer_IntegrationTests";
    private const string TestConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;" +
        $"Database={TestDatabaseName};" +
        "Trusted_Connection=True;" +
        "TrustServerCertificate=True;" +
        "MultipleActiveResultSets=true";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["RapidApi:BaseUrl"] = "https://example.com",
                ["RapidApi:ApiHost"] = "example.com",
                ["RapidApi:ApiKey"] = "integration-test-api-key"
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
