using AmazonBestSellersExplorer.Domain.Entities;
using Xunit;

namespace AmazonBestSellersExplorer.UnitTests.Domain.Entities;

public sealed class AuditLogTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var userId = Guid.NewGuid();

        var result = AuditLog.Create("FavoriteAdded", "FavoriteProduct", "B09TEST001", userId);

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var auditLog));
        Assert.NotNull(auditLog);
        Assert.NotEqual(Guid.Empty, auditLog.Id);
        Assert.Equal(userId, auditLog.UserId);
        Assert.Equal("FavoriteAdded", auditLog.Action);
        Assert.Equal("FavoriteProduct", auditLog.EntityType);
        Assert.Equal("B09TEST001", auditLog.EntityId);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenActionIsEmpty()
    {
        var result = AuditLog.Create(string.Empty, "FavoriteProduct");

        Assert.True(result.IsFailed);
        Assert.Contains("Action is required.", result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenEntityTypeIsEmpty()
    {
        var result = AuditLog.Create("FavoriteAdded", string.Empty);

        Assert.True(result.IsFailed);
        Assert.Contains("Entity type is required.", result.Errors);
    }
}
