using AmazonBestSellersExplorer.Domain.Abstraction;
using AmazonBestSellersExplorer.Domain.Base;

namespace AmazonBestSellersExplorer.Domain.Entities;

public sealed class AuditLog : Entity<Guid>
{
    public Guid? UserId { get; }

    public string Action { get; }

    public string EntityType { get; }

    public string? EntityId { get; }

    private AuditLog(Guid id, Guid? userId, string action, string entityType, string? entityId, DateTime createdAtUtc) : base(id, createdAtUtc)
    {
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
    }


    public static Result<AuditLog> Create(string action, string entityType, string? entityId = null, Guid? userId = null)
    {
        var id = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;
        var errors = Validate(id, action, entityType);

        return errors.Count > 0 ? Result.Fail<AuditLog>(errors) : Result.Success(new AuditLog(id, userId, action, entityType, entityId, createdAtUtc));
    }

    private static List<string> Validate(Guid id, string action, string entityType)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("Audit log id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            errors.Add("Action is required.");
        }

        if (string.IsNullOrWhiteSpace(entityType))
        {
            errors.Add("Entity type is required.");
        }

        return errors;
    }
}
