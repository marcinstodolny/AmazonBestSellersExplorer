using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AmazonBestSellersExplorer.IntegrationTests.Infrastructure;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.AuthAndFavorites;

public sealed class AuthAndFavoritesFlowTests(IntegrationTestWebApplicationFactory factory)
    : IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory = factory;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ShouldReturn200AndToken()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(
                Username: GenerateUsername(),
                Password: "StrongPassword1"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.True(body.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldReturn200AndToken_ForValidCredentials()
    {
        var username = GenerateUsername();
        const string password = "StrongPassword1";

        await RegisterAsync(username, password);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(
                Username: username,
                Password: password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.True(body.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task AddFavorite_ShouldWork_ForAuthorizedUser()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST001"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFavorites_ShouldReturnSavedProduct()
    {
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST002"));

        var response = await client.GetAsync("/api/favorites");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<FavoriteProductDto>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("B09TEST002", body[0].AmazonProductId);
        Assert.Equal("Windows 11 Pro", body[0].Title);
    }

    [Fact]
    public async Task RemoveFavorite_ShouldDeleteSavedProduct()
    {
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST003"));

        var deleteResponse = await client.DeleteAsync("/api/favorites/B09TEST003");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/favorites");
        var body = await getResponse.Content.ReadFromJsonAsync<List<FavoriteProductDto>>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    private async Task RegisterAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(username, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var username = GenerateUsername();
        const string password = "StrongPassword1";

        await RegisterAsync(username, password);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(username, password));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));

        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

        return authenticatedClient;
    }

    private static AddFavoriteProductRequest CreateFavoriteRequest(string amazonProductId)
    {
        return new AddFavoriteProductRequest(
            AmazonProductId: amazonProductId,
            Title: "Windows 11 Pro",
            Price: 699.99m,
            Rating: 4.7,
            ProductUrl: "https://www.amazon.pl/dp/" + amazonProductId,
            ImageUrl: "https://images.example.com/" + amazonProductId + ".jpg");
    }

    private static string GenerateUsername()
    {
        return "user" + Guid.NewGuid().ToString("N")[..12];
    }

    private sealed record RegisterUserRequest(string Username, string Password);

    private sealed record LoginUserRequest(string Username, string Password);

    private sealed record AuthResponseDto(string AccessToken, DateTime ExpiresAtUtc);

    private sealed record AddFavoriteProductRequest(
        string AmazonProductId,
        string Title,
        decimal? Price,
        double? Rating,
        string ProductUrl,
        string? ImageUrl);

    private sealed record FavoriteProductDto(
        string AmazonProductId,
        string Title,
        decimal? Price,
        double? Rating,
        string ProductUrl,
        string? ImageUrl);
}
