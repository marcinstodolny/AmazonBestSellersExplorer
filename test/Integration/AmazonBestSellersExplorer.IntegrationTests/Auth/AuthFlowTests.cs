using System.Net;
using System.Net.Http.Json;
using AmazonBestSellersExplorer.IntegrationTests.Infrastructure;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthFlowTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture = fixture;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        _client = _fixture.CreateClient();
        await _fixture.ResetAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ShouldReturn200AndToken()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(TestAuthHelper.GenerateUsername(), TestAuthHelper.DefaultPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.True(body.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenUsernameIsAlreadyTaken()
    {
        var username = TestAuthHelper.GenerateUsername();
        const string password = TestAuthHelper.DefaultPassword;

        await TestAuthHelper.RegisterAsync(_client, username, password);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(username, password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturn200AndToken_ForValidCredentials()
    {
        var username = TestAuthHelper.GenerateUsername();
        const string password = TestAuthHelper.DefaultPassword;

        await TestAuthHelper.RegisterAsync(_client, username, password);

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
        var username = TestAuthHelper.GenerateUsername();

        await TestAuthHelper.RegisterAsync(_client, username, TestAuthHelper.DefaultPassword);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(username, "WrongPassword1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

}
