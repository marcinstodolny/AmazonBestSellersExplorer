using System.Net;
using System.Net.Http.Json;
using AmazonBestSellersExplorer.Domain.Entities;
using AmazonBestSellersExplorer.IntegrationTests.Infrastructure;
using AmazonBestSellersExplorer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthFlowTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = fixture.CreateClient();
        await fixture.ResetAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ShouldReturn200AndToken()
    {
        var username = TestAuthHelper.GenerateUsername();

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(username, TestAuthHelper.DefaultPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.True(body.ExpiresAtUtc > DateTime.UtcNow);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync();
        var auditLog = await dbContext.AuditLogs.SingleAsync(log => log.Action == "UserRegistered");

        Assert.Equal(nameof(User), auditLog.EntityType);
        Assert.Equal(user.Id.ToString(), auditLog.EntityId);
        Assert.Equal(user.Id, auditLog.UserId);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenUsernameIsAlreadyTaken()
    {
        var username = TestAuthHelper.GenerateUsername();

        await TestAuthHelper.RegisterAsync(_client, username);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(username, TestAuthHelper.DefaultPassword));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.ReadProblemDetailsAsync();

        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.Equal("Request validation failed.", problemDetails.Title);
        Assert.NotNull(problemDetails.Detail);
        Assert.Contains("Username is already taken.", problemDetails.Detail);
    }

    [Fact]
    public async Task Login_ShouldReturn200AndToken_ForValidCredentials()
    {
        var username = TestAuthHelper.GenerateUsername();

        await TestAuthHelper.RegisterAsync(_client, username);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(username, TestAuthHelper.DefaultPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.True(body.ExpiresAtUtc > DateTime.UtcNow);

        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync();
        var auditLog = await dbContext.AuditLogs.SingleAsync(log => log.Action == "UserLoggedIn");

        Assert.Equal(nameof(User), auditLog.EntityType);
        Assert.Equal(user.Id.ToString(), auditLog.EntityId);
        Assert.Equal(user.Id, auditLog.UserId);
    }

    [Fact]
    public async Task Login_ShouldReturn401_ForInvalidPassword()
    {
        var username = TestAuthHelper.GenerateUsername();

        await TestAuthHelper.RegisterAsync(_client, username);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(username, "WrongPassword1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var problemDetails = await response.ReadProblemDetailsAsync();

        Assert.Equal((int)HttpStatusCode.Unauthorized, problemDetails.Status);
        Assert.Equal("Authentication failed.", problemDetails.Title);
        Assert.NotNull(problemDetails.Detail);
        Assert.Contains("Invalid username or password.", problemDetails.Detail);
    }

    [Fact]
    public async Task Login_ShouldReturn400_WhenRequestValidationFails()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(string.Empty, string.Empty));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.ReadProblemDetailsAsync();

        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.Equal("Request validation failed.", problemDetails.Title);
        Assert.NotNull(problemDetails.Detail);
        Assert.Contains("Username is required.", problemDetails.Detail);
        Assert.Contains("Password is required.", problemDetails.Detail);
    }

}
