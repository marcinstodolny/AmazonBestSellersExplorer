using AmazonBestSellersExplorer.Domain.Abstraction;
using AmazonBestSellersExplorer.Domain.Base;

namespace AmazonBestSellersExplorer.Domain.Entities;

public sealed class FavoriteProduct : Entity<Guid>
{
    public Guid UserId { get; }

    public string AmazonProductId { get; private set; }

    public string Title { get; private set; }

    public decimal? Price { get; private set; }

    public double? Rating { get; private set; }

    public string ProductUrl { get; private set; }

    public string? ImageUrl { get; private set; }

    private FavoriteProduct(Guid id, Guid userId, string amazonProductId, string title, decimal? price, double? rating, string productUrl, string? imageUrl, DateTime createdAtUtc) : base(id, createdAtUtc)
    {
        UserId = userId;
        AmazonProductId = amazonProductId;
        Title = title;
        Price = price;
        Rating = rating;
        ProductUrl = productUrl;
        ImageUrl = imageUrl;
    }

    public static Result<FavoriteProduct> Create(Guid userId, string amazonProductId, string title, decimal? price, double? rating, string productUrl, string? imageUrl)
    {
        var id = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;
        var errors = Validate(id, userId, amazonProductId, title, productUrl);

        if (errors.Count > 0)
        {
            return Result.Fail<FavoriteProduct>(errors);
        }

        return Result.Success(new FavoriteProduct(
            id,
            userId,
            amazonProductId,
            title,
            price,
            rating,
            productUrl,
            imageUrl,
            createdAtUtc));
    }

    public Result Update(string amazonProductId, string title, decimal? price, double? rating, string productUrl, string? imageUrl)
    {
        var errors = Validate(Id, UserId, amazonProductId, title, productUrl);

        if (errors.Count > 0)
        {
            return Result.Fail(errors);
        }

        AmazonProductId = amazonProductId;
        Title = title;
        Price = price;
        Rating = rating;
        ProductUrl = productUrl;
        ImageUrl = imageUrl;

        return Result.Success();
    }

    private static IReadOnlyCollection<string> Validate(Guid id, Guid userId, string amazonProductId, string title, string productUrl)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("Favorite product id cannot be empty.");
        }

        if (userId == Guid.Empty)
        {
            errors.Add("User id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(amazonProductId))
        {
            errors.Add("Amazon product id is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(productUrl))
        {
            errors.Add("Product URL is required.");
        }

        return errors;
    }
}
