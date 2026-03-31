using AmazonBestSellersExplorer.Domain.ValueObjects;
using Xunit;

namespace AmazonBestSellersExplorer.UnitTests.Domain.ValueObjects;

public sealed class UsernameTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        const string value = "Marcin123";

        var result = Username.Create(value);

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var username));
        Assert.NotNull(username);
        Assert.Equal(value, username.Value);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUsernameIsEmpty()
    {
        var result = Username.Create(string.Empty);

        Assert.True(result.IsFailed);
        Assert.Contains("Username is required.", result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUsernameLengthIsInvalid()
    {
        var result = Username.Create("abc");

        Assert.True(result.IsFailed);
        Assert.Contains("Username must be between 5 and 50 characters.", result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUsernameContainsInvalidCharacters()
    {
        var result = Username.Create("marcin!");

        Assert.True(result.IsFailed);
        Assert.Contains("Username can contain only ASCII letters and digits.", result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUsernameContainsWhitespace()
    {
        var result = Username.Create("Marcin 123");

        Assert.True(result.IsFailed);
        Assert.Contains("Username can contain only ASCII letters and digits.", result.Errors);
    }

    [Fact]
    public void FromPersistence_ShouldReturnUsername_WhenValueIsValid()
    {
        var username = Username.FromPersistence("Marcin123");

        Assert.Equal("Marcin123", username.Value);
    }

    [Fact]
    public void FromPersistence_ShouldThrow_WhenValueIsInvalid()
    {
        var action = () => Username.FromPersistence("!");

        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal("Persisted username value is invalid.", exception.Message);
    }
}
