using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AmazonBestSellersExplorer.IntegrationTests.Infrastructure;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Favorites;

[Collection(IntegrationTestCollection.Name)]
public sealed class FavoritesFlowTests(IntegrationTestWebApplicationFactory factory)
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
    public async Task GetFavorites_ShouldReturn401_WithoutToken()
    {
        var response = await _client.GetAsync("/api/favorites");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
    public async Task AddFavorite_ShouldReturn400_WhenProductAlreadyExists()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = CreateFavoriteRequest("B09TEST002");

        var firstResponse = await client.PostAsJsonAsync("/api/favorites", request);
        var secondResponse = await client.PostAsJsonAsync("/api/favorites", request);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GetFavorites_ShouldReturnSavedProduct()
    {
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST003"));

        var response = await client.GetAsync("/api/favorites");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<FavoriteProductDto>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("B09TEST003", body[0].AmazonProductId);
        Assert.Equal("Windows 11 Pro", body[0].Title);
    }

    [Fact]
    public async Task RemoveFavorite_ShouldDeleteSavedProduct()
    {
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST004"));

        var deleteResponse = await client.DeleteAsync("/api/favorites/B09TEST004");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/favorites");
        var body = await getResponse.Content.ReadFromJsonAsync<List<FavoriteProductDto>>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var username = GenerateUsername();
        const string password = "StrongPassword1";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(username, password));

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

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
