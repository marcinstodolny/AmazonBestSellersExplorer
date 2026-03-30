using AmazonBestSellersExplorer.Domain.Abstraction;
using AmazonBestSellersExplorer.Domain.Base;

namespace AmazonBestSellersExplorer.Domain.Entities;

public sealed class User : Entity<Guid>
{
    private readonly List<FavoriteProduct> _favoriteProducts;

    public string Username { get; private set; }

    public string PasswordHash { get; private set; }

    public IReadOnlyCollection<FavoriteProduct> FavoriteProducts => _favoriteProducts;

    private User(
        Guid id,
        string username,
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
        var errors = Validate(id, username, passwordHash);

        if (errors.Count > 0)
        {
            return Result.Fail<User>(errors);
        }

        return Result.Success(new User(
            id,
            username,
            passwordHash,
            createdAtUtc));
    }

    public Result Update(string username, string passwordHash)
    {
        var errors = Validate(Id, username, passwordHash);

        if (errors.Count > 0)
        {
            return Result.Fail(errors);
        }

        Username = username;
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

    private static IReadOnlyCollection<string> Validate(Guid id, string username, string passwordHash)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("User id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Add("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            errors.Add("Password hash is required.");
        }

        return errors;
    }
}
