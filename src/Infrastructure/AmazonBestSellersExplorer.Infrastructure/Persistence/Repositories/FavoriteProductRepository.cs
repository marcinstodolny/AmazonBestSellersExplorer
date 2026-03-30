using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AmazonBestSellersExplorer.Infrastructure.Persistence.Repositories;

public sealed class FavoriteProductRepository(AppDbContext dbContext) : IFavoriteProductRepository
{
    public async Task<IReadOnlyList<FavoriteProduct>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.FavoriteProducts
            .Where(favoriteProduct => favoriteProduct.UserId == userId)
            .OrderByDescending(favoriteProduct => favoriteProduct.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<FavoriteProduct?> GetByUserIdAndAmazonProductIdAsync(
        Guid userId,
        string amazonProductId,
        CancellationToken cancellationToken)
    {
        return dbContext.FavoriteProducts
            .FirstOrDefaultAsync(
                favoriteProduct => favoriteProduct.UserId == userId
                    && favoriteProduct.AmazonProductId == amazonProductId,
                cancellationToken);
    }

    public Task AddAsync(FavoriteProduct favoriteProduct, CancellationToken cancellationToken)
    {
        return dbContext.FavoriteProducts.AddAsync(favoriteProduct, cancellationToken).AsTask();
    }

    public void Remove(FavoriteProduct favoriteProduct)
    {
        dbContext.FavoriteProducts.Remove(favoriteProduct);
    }
}
