using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Domain.Entities;

namespace AmazonBestSellersExplorer.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(AppDbContext dbContext) : IAuditLogRepository
{
    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken)
    {
        return dbContext.AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();
    }
}
