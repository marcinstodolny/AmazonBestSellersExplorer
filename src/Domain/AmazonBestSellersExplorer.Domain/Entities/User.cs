using AmazonBestSellersExplorer.Domain.Abstraction;
using AmazonBestSellersExplorer.Domain.Base;
using AmazonBestSellersExplorer.Domain.ValueObjects;

namespace AmazonBestSellersExplorer.Domain.Entities;

public sealed class User : Entity<Guid>
{
    private readonly List<FavoriteProduct> _favoriteProducts;

    public Username Username { get; private set; }

    public string PasswordHash { get; private set; }

    public IReadOnlyCollection<FavoriteProduct> FavoriteProducts => _favoriteProducts;

    private User(
        Guid id,
        Username username,
        string passwordHash,
        DateTime createdAtUtc,
        IEnumerable<FavoriteProduct>? favoriteProducts = null)
        : base(id, createdAtUtc)
    {
        Username = username;
        PasswordHash = passwordHash;
        _favoriteProducts = favoriteProducts?.ToList() ?? [];
    }

    public static Result<User> Create(string username, string passwordHash)
    {
        var id = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;
        var usernameResult = Username.Create(username);
        var errors = Validate(id, usernameResult, passwordHash);

        if (errors.Count > 0)
        {
            return Result.Fail<User>(errors);
        }

        if (!usernameResult.TryGetValue(out var usernameValue))
        {
            return Result.Fail<User>(usernameResult.Errors);
        }

        return Result.Success(new User(
            id,
            usernameValue,
            passwordHash,
            createdAtUtc));
    }

    public Result Update(string username, string passwordHash)
    {
        var usernameResult = Username.Create(username);
        var errors = Validate(Id, usernameResult, passwordHash);

        if (errors.Count > 0)
        {
            return Result.Fail(errors);
        }

        if (!usernameResult.TryGetValue(out var usernameValue))
        {
            return Result.Fail(usernameResult.Errors);
        }

        Username = usernameValue;
        PasswordHash = passwordHash;

        return Result.Success();
    }

    public Result<FavoriteProduct> AddFavoriteProduct(
        string amazonProductId,
        string title,
        decimal? price,
        double? rating,
        string productUrl,
        string? imageUrl)
    {
        if (HasFavorite(amazonProductId))
        {
            return Result.Fail<FavoriteProduct>("Product is already in favorites.");
        }

        var favoriteProductResult = FavoriteProduct.Create(
            Id,
            amazonProductId,
            title,
            price,
            rating,
            productUrl,
            imageUrl);

        if (favoriteProductResult.IsFailed || !favoriteProductResult.TryGetValue(out var favoriteProduct))
        {
            return favoriteProductResult;
        }

        _favoriteProducts.Add(favoriteProduct);

        return favoriteProductResult;
    }

    public bool HasFavorite(string amazonProductId)
    {
        return !string.IsNullOrWhiteSpace(amazonProductId)
            && FindFavoriteProduct(amazonProductId) is not null;
    }

    public Result RemoveFavoriteProduct(string amazonProductId)
    {
        if (string.IsNullOrWhiteSpace(amazonProductId))
        {
            return Result.Fail("Amazon product id is required.");
        }

        var favoriteProduct = FindFavoriteProduct(amazonProductId);
        if (favoriteProduct is null)
        {
            return Result.Fail("Favorite product was not found.");
        }

        _favoriteProducts.Remove(favoriteProduct);

        return Result.Success();
    }

    private static IReadOnlyCollection<string> Validate(Guid id, Result<Username> usernameResult, string passwordHash)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("User id cannot be empty.");
        }

        if (usernameResult.IsFailed)
        {
            errors.AddRange(usernameResult.Errors);
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            errors.Add("Password hash is required.");
        }

        return errors;
    }

    private FavoriteProduct? FindFavoriteProduct(string amazonProductId)
    {
        return _favoriteProducts.FirstOrDefault(product =>
            string.Equals(product.AmazonProductId, amazonProductId, StringComparison.OrdinalIgnoreCase));
    }
}
