using AmazonBestSellersExplorer.Domain.Entities;

namespace AmazonBestSellersExplorer.Application.Abstractions.Persistence;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken);
}
