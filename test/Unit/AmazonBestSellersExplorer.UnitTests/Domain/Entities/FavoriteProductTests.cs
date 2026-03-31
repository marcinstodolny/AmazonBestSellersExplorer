using AmazonBestSellersExplorer.Domain.Entities;
using Xunit;

namespace AmazonBestSellersExplorer.UnitTests.Domain.Entities;

public sealed class FavoriteProductTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var userId = Guid.NewGuid();

        var result = FavoriteProduct.Create(
            userId,
            "B09TEST001",
            "Windows 11 Pro",
            699.99m,
            4.7,
            "https://www.amazon.pl/dp/B09TEST001",
            "https://images.example.com/B09TEST001.jpg");

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var favoriteProduct));
        Assert.NotNull(favoriteProduct);
        Assert.NotEqual(Guid.Empty, favoriteProduct.Id);
        Assert.Equal(userId, favoriteProduct.UserId);
        Assert.Equal("B09TEST001", favoriteProduct.AmazonProductId);
        Assert.Equal("Windows 11 Pro", favoriteProduct.Title);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUserIdIsEmpty()
    {
        var result = FavoriteProduct.Create(
            Guid.Empty,
            "B09TEST001",
            "Windows 11 Pro",
            699.99m,
            4.7,
            "https://www.amazon.pl/dp/B09TEST001",
            "https://images.example.com/B09TEST001.jpg");

        Assert.True(result.IsFailed);
        Assert.Contains("User id cannot be empty.", result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenRequiredFieldsAreMissing()
    {
        var result = FavoriteProduct.Create(
            Guid.NewGuid(),
            string.Empty,
            string.Empty,
            699.99m,
            4.7,
            string.Empty,
            null);

        Assert.True(result.IsFailed);
        Assert.Contains("Amazon product id is required.", result.Errors);
        Assert.Contains("Title is required.", result.Errors);
        Assert.Contains("Product URL is required.", result.Errors);
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WhenDataIsValid()
    {
        var createResult = FavoriteProduct.Create(
            Guid.NewGuid(),
            "B09TEST001",
            "Windows 11 Pro",
            699.99m,
            4.7,
            "https://www.amazon.pl/dp/B09TEST001",
            "https://images.example.com/B09TEST001.jpg");

        Assert.True(createResult.TryGetValue(out var favoriteProduct));

        var result = favoriteProduct!.Update(
            "B09TEST002",
            "Office 365",
            499.99m,
            4.5,
            "https://www.amazon.pl/dp/B09TEST002",
            "https://images.example.com/B09TEST002.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal("B09TEST002", favoriteProduct.AmazonProductId);
        Assert.Equal("Office 365", favoriteProduct.Title);
        Assert.Equal(499.99m, favoriteProduct.Price);
        Assert.Equal(4.5, favoriteProduct.Rating);
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenRequiredFieldsAreMissing()
    {
        var createResult = FavoriteProduct.Create(
            Guid.NewGuid(),
            "B09TEST001",
            "Windows 11 Pro",
            699.99m,
            4.7,
            "https://www.amazon.pl/dp/B09TEST001",
            "https://images.example.com/B09TEST001.jpg");

        Assert.True(createResult.TryGetValue(out var favoriteProduct));

        var result = favoriteProduct!.Update(
            string.Empty,
            string.Empty,
            499.99m,
            4.5,
            string.Empty,
            null);

        Assert.True(result.IsFailed);
        Assert.Contains("Amazon product id is required.", result.Errors);
        Assert.Contains("Title is required.", result.Errors);
        Assert.Contains("Product URL is required.", result.Errors);
    }
}
