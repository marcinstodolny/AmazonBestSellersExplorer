using System.Net;
using System.Net.Http.Json;
using AmazonBestSellersExplorer.IntegrationTests.Infrastructure;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthFlowTests(IntegrationTestWebApplicationFactory factory)
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
            new RegisterUserRequest(GenerateUsername(), "StrongPassword1"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.True(body.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenUsernameIsAlreadyTaken()
    {
        var username = GenerateUsername();
        const string password = "StrongPassword1";

        await RegisterAsync(username, password);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(username, password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturn200AndToken_ForValidCredentials()
    {
        var username = GenerateUsername();
        const string password = "StrongPassword1";

        await RegisterAsync(username, password);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(username, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.True(body.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldReturn401_ForInvalidPassword()
    {
        var username = GenerateUsername();

        await RegisterAsync(username, "StrongPassword1");

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(username, "WrongPassword1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task RegisterAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(username, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string GenerateUsername()
    {
        return "user" + Guid.NewGuid().ToString("N")[..12];
    }

    private sealed record RegisterUserRequest(string Username, string Password);

    private sealed record LoginUserRequest(string Username, string Password);

    private sealed record AuthResponseDto(string AccessToken, DateTime ExpiresAtUtc);
}
