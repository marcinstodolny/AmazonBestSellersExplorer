using System.Net.Http.Headers;
using AmazonBestSellersExplorer.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly string _databaseName = $"AmazonBestSellersExplorer_IntegrationTests_{Guid.NewGuid():N}";
    private SqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public IntegrationTestWebApplicationFactory Factory { get; private set; } = null!;

    private string ConnectionString =>
        "Server=(localdb)\\MSSQLLocalDB;" +
        $"Database={_databaseName};" +
        "Trusted_Connection=True;" +
        "TrustServerCertificate=True;" +
        "MultipleActiveResultSets=true";

    public async Task InitializeAsync()
    {
        Factory = new IntegrationTestWebApplicationFactory(ConnectionString);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actualConnectionString = dbContext.Database.GetConnectionString();

        if (actualConnectionString is null
            || !actualConnectionString.Contains(_databaseName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Integration tests are not using the expected test database.");
        }

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();

        _connection = new SqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"],
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureDeletedAsync();
    }

    public HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    public async Task ResetAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var anonymousClient = CreateClient();
        var username = TestAuthHelper.GenerateUsername();

        await TestAuthHelper.RegisterAsync(anonymousClient, username);
        var authResponse = await TestAuthHelper.LoginAsync(anonymousClient, username);

        var authenticatedClient = CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

        return authenticatedClient;
    }
}
