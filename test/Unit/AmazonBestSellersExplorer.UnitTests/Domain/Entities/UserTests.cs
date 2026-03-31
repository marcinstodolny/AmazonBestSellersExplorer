using AmazonBestSellersExplorer.Domain.Entities;
using Xunit;

namespace AmazonBestSellersExplorer.UnitTests.Domain.Entities;

public sealed class UserTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        const string username = "Marcin123";
        const string passwordHash = "hashed-password";

        var result = User.Create(username, passwordHash);

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var user));
        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(username, user.Username.Value);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Empty(user.FavoriteProducts);
        Assert.True(user.CreatedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUsernameIsInvalid()
    {
        var result = User.Create(string.Empty, "hashed-password");

        Assert.True(result.IsFailed);
        Assert.Contains("Username is required.", result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPasswordHashIsEmpty()
    {
        var result = User.Create("Marcin123", string.Empty);

        Assert.True(result.IsFailed);
        Assert.Contains("Password hash is required.", result.Errors);
    }

    [Fact]
    public void AddFavoriteProduct_ShouldReturnFailure_WhenFavoriteAlreadyExists()
    {
        var userResult = User.Create("Marcin123", "hashed-password");

        Assert.True(userResult.TryGetValue(out var user));

        var firstResult = user!.AddFavoriteProduct(
            "B09TEST001",
            "Windows 11 Pro",
            699.99m,
            4.7,
            "https://www.amazon.pl/dp/B09TEST001",
            "https://images.example.com/B09TEST001.jpg");

        var secondResult = user.AddFavoriteProduct(
            "b09test001",
            "Windows 11 Pro",
            699.99m,
            4.7,
            "https://www.amazon.pl/dp/B09TEST001",
            "https://images.example.com/B09TEST001.jpg");

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsFailed);
        Assert.Contains("Product is already in favorites.", secondResult.Errors);
    }

    [Fact]
    public void HasFavorite_ShouldReturnTrue_WhenFavoriteExistsWithDifferentCasing()
    {
        var userResult = User.Create("Marcin123", "hashed-password");

        Assert.True(userResult.TryGetValue(out var user));

        var addResult = user!.AddFavoriteProduct(
            "B09TEST001",
            "Windows 11 Pro",
            699.99m,
            4.7,
            "https://www.amazon.pl/dp/B09TEST001",
            "https://images.example.com/B09TEST001.jpg");

        Assert.True(addResult.IsSuccess);
        Assert.True(user.HasFavorite("b09test001"));
    }

    [Fact]
    public void RemoveFavoriteProduct_ShouldReturnSuccess_WhenFavoriteExists()
    {
        var userResult = User.Create("Marcin123", "hashed-password");

        Assert.True(userResult.TryGetValue(out var user));

        var addResult = user!.AddFavoriteProduct(
            "B09TEST001",
            "Windows 11 Pro",
            699.99m,
            4.7,
            "https://www.amazon.pl/dp/B09TEST001",
            "https://images.example.com/B09TEST001.jpg");

        Assert.True(addResult.IsSuccess);

        var result = user.RemoveFavoriteProduct("b09test001");

        Assert.True(result.IsSuccess);
        Assert.Empty(user.FavoriteProducts);
    }

    [Fact]
    public void RemoveFavoriteProduct_ShouldReturnFailure_WhenAmazonProductIdIsEmpty()
    {
        var userResult = User.Create("Marcin123", "hashed-password");

        Assert.True(userResult.TryGetValue(out var user));

        var result = user!.RemoveFavoriteProduct(string.Empty);

        Assert.True(result.IsFailed);
        Assert.Contains("Amazon product id is required.", result.Errors);
    }

    [Fact]
    public void RemoveFavoriteProduct_ShouldReturnFailure_WhenFavoriteDoesNotExist()
    {
        var userResult = User.Create("Marcin123", "hashed-password");

        Assert.True(userResult.TryGetValue(out var user));

        var result = user!.RemoveFavoriteProduct("B09UNKNOWN");

        Assert.True(result.IsFailed);
        Assert.Contains("Favorite product was not found.", result.Errors);
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WhenDataIsValid()
    {
        var userResult = User.Create("Marcin123", "hashed-password");

        Assert.True(userResult.TryGetValue(out var user));

        var result = user!.Update("Updated123", "new-hash");

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated123", user.Username.Value);
        Assert.Equal("new-hash", user.PasswordHash);
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenPasswordHashIsEmpty()
    {
        var userResult = User.Create("Marcin123", "hashed-password");

        Assert.True(userResult.TryGetValue(out var user));

        var result = user!.Update("Updated123", string.Empty);

        Assert.True(result.IsFailed);
        Assert.Contains("Password hash is required.", result.Errors);
    }
}
