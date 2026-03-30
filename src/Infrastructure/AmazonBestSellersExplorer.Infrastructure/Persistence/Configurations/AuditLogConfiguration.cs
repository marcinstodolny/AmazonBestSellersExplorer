using AmazonBestSellersExplorer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmazonBestSellersExplorer.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(auditLog => auditLog.Id);

        builder.Property(auditLog => auditLog.Id)
            .ValueGeneratedNever();

        builder.Property(auditLog => auditLog.Action)
            .IsRequired();

        builder.Property(auditLog => auditLog.EntityType)
            .IsRequired();

        builder.Property(auditLog => auditLog.EntityId)
            .IsRequired(false);

        builder.Property(auditLog => auditLog.CreatedAtUtc)
            .IsRequired();

        builder.Property(auditLog => auditLog.UserId)
            .IsRequired(false);
    }
}
