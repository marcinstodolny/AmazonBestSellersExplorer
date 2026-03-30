namespace AmazonBestSellersExplorer.Domain.Abstraction;

public abstract class Entity<TId>(TId id, DateTime createdAtUtc)
{
    public TId Id { get; } = id;

    public DateTime CreatedAtUtc { get; } = createdAtUtc;
}
