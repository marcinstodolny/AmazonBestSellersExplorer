using AmazonBestSellersExplorer.Domain.Entities;

namespace AmazonBestSellersExplorer.Application.Abstractions.Persistence;

public interface IFavoriteProductRepository
{
    Task<IReadOnlyList<FavoriteProduct>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<FavoriteProduct?> GetByUserIdAndAmazonProductIdAsync(
        Guid userId,
        string amazonProductId,
        CancellationToken cancellationToken);

    Task AddAsync(FavoriteProduct favoriteProduct, CancellationToken cancellationToken);

    void Remove(FavoriteProduct favoriteProduct);
}
