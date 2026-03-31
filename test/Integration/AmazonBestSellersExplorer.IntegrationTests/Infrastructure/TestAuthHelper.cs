using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Infrastructure;

internal static class TestAuthHelper
{
    public const string DefaultPassword = "StrongPassword1";

    public static string GenerateUsername()
    {
        return "user" + Guid.NewGuid().ToString("N")[..12];
    }

    public static async Task RegisterAsync(HttpClient client, string username, string password = DefaultPassword)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(username, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public static async Task<AuthResponseDto> LoginAsync(HttpClient client, string username, string password = DefaultPassword)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(username, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.True(authResponse.ExpiresAtUtc > DateTime.UtcNow);

        return authResponse;
    }
}
