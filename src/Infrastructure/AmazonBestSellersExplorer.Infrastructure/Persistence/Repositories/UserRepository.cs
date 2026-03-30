using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Domain.Entities;
using AmazonBestSellersExplorer.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AmazonBestSellersExplorer.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(user => user.FavoriteProducts)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var usernameResult = Username.Create(username);
        if (!usernameResult.TryGetValue(out var usernameValue))
        {
            return null;
        }

        return await dbContext.Users
            .Include(user => user.FavoriteProducts)
            .FirstOrDefaultAsync(user => user.Username == usernameValue, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var usernameResult = Username.Create(username);
        if (!usernameResult.TryGetValue(out var usernameValue))
        {
            return false;
        }

        return await dbContext.Users
            .AnyAsync(user => user.Username == usernameValue, cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }
}
