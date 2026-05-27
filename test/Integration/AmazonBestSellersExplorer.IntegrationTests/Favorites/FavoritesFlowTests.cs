using System.Net;
using System.Net.Http.Json;
using AmazonBestSellersExplorer.IntegrationTests.Infrastructure;
using Xunit;

namespace AmazonBestSellersExplorer.IntegrationTests.Favorites;

[Collection(IntegrationTestCollection.Name)]
public sealed class FavoritesFlowTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = fixture.CreateClient();
        await fixture.ResetAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetFavorites_ShouldReturn401_WithoutToken()
    {
        var response = await _client.GetAsync("/api/favorites");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn401_WithoutToken()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09UNAUTH001"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_ShouldWork_ForAuthorizedUser()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST001"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn400_WhenProductAlreadyExists()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();
        var request = CreateFavoriteRequest("B09TEST002");

        var firstResponse = await client.PostAsJsonAsync("/api/favorites", request);
        var secondResponse = await client.PostAsJsonAsync("/api/favorites", request);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);

        var problemDetails = await secondResponse.ReadProblemDetailsAsync();

        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.Equal("Favorites request failed.", problemDetails.Title);
        Assert.NotNull(problemDetails.Detail);
        Assert.Contains("Favorite product already exists.", problemDetails.Detail);
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn400_WhenProductAlreadyExistsWithDifferentCasing()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();

        var firstResponse = await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TESTCASE"));
        var secondResponse = await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("b09testcase"));

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);

        var problemDetails = await secondResponse.ReadProblemDetailsAsync();

        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.Equal("Favorites request failed.", problemDetails.Title);
        Assert.NotNull(problemDetails.Detail);
        Assert.Contains("Favorite product already exists.", problemDetails.Detail);
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn400_WhenAmazonProductIdExceedsMaximumLength()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();
        var request = CreateFavoriteRequest(new string('A', 65));

        var response = await client.PostAsJsonAsync("/api/favorites", request);

        await AssertFavoriteValidationErrorAsync(
            response,
            "Amazon product id must not exceed 64 characters.");
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn400_WhenPriceIsNegative()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();
        var request = CreateFavoriteRequest("B09TESTNEG1", price: -1m);

        var response = await client.PostAsJsonAsync("/api/favorites", request);

        await AssertFavoriteValidationErrorAsync(response, "Price cannot be negative.");
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn400_WhenRatingIsOutOfRange()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();
        var request = CreateFavoriteRequest("B09TESTNEG2", rating: 7);

        var response = await client.PostAsJsonAsync("/api/favorites", request);

        await AssertFavoriteValidationErrorAsync(response, "Rating must be between 0 and 5.");
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn400_WhenProductUrlIsMalformed()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();
        var request = CreateFavoriteRequest("B09TESTNEG3", productUrl: "not-a-url");

        var response = await client.PostAsJsonAsync("/api/favorites", request);

        await AssertFavoriteValidationErrorAsync(response, "Product URL must be a valid absolute URL.");
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn400_WhenImageUrlIsMalformed()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();
        var request = CreateFavoriteRequest("B09TESTNEG4", imageUrl: "invalid-image");

        var response = await client.PostAsJsonAsync("/api/favorites", request);

        await AssertFavoriteValidationErrorAsync(response, "Image URL must be a valid absolute URL.");
    }

    [Fact]
    public async Task GetFavorites_ShouldReturnSavedProduct()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST003"));

        var response = await client.GetAsync("/api/favorites");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<FavoriteProductResponse>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("B09TEST003", body[0].AmazonProductId);
        Assert.Equal("Windows 11 Pro", body[0].Title);
    }

    [Fact]
    public async Task RemoveFavorite_ShouldDeleteSavedProduct()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST004"));

        var deleteResponse = await client.DeleteAsync("/api/favorites/B09TEST004");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/favorites");
        var body = await getResponse.Content.ReadFromJsonAsync<List<FavoriteProductResponse>>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task RemoveFavorite_ShouldDeleteSavedProduct_WhenProductIdCasingDiffers()
    {
        var client = await fixture.CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync(
            "/api/favorites",
            CreateFavoriteRequest("B09TEST005"));

        var deleteResponse = await client.DeleteAsync("/api/favorites/b09test005");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/favorites");
        var body = await getResponse.Content.ReadFromJsonAsync<List<FavoriteProductResponse>>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task RemoveFavorite_ShouldReturn401_WithoutToken()
    {
        var response = await _client.DeleteAsync("/api/favorites/B09UNAUTH002");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static AddFavoriteProductRequest CreateFavoriteRequest(
        string amazonProductId,
        decimal? price = 699.99m,
        double? rating = 4.7,
        string productUrl = "",
        string? imageUrl = "")
    {
        return new AddFavoriteProductRequest(
            AmazonProductId: amazonProductId,
            Title: "Windows 11 Pro",
            Price: price,
            Rating: rating,
            ProductUrl: string.IsNullOrWhiteSpace(productUrl)
                ? "https://www.amazon.pl/dp/" + amazonProductId
                : productUrl,
            ImageUrl: string.IsNullOrWhiteSpace(imageUrl)
                ? "https://images.example.com/" + amazonProductId + ".jpg"
                : imageUrl);
    }

    private static async Task AssertFavoriteValidationErrorAsync(HttpResponseMessage response, string expectedError)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.ReadProblemDetailsAsync();

        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.Equal("Favorites request failed.", problemDetails.Title);
        Assert.NotNull(problemDetails.Detail);
        Assert.Contains(expectedError, problemDetails.Detail);
    }
}
