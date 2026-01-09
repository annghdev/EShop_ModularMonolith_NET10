using Inventory.Domain;
using Kernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EFCore.Configurations;

public class StockLogConfiguration : IEntityTypeConfiguration<StockLog>
{
    public void Configure(EntityTypeBuilder<StockLog> builder)
    {
        // Base entity configuration
        builder.HasKey(l => l.Id);

        // Properties
        builder.Property(l => l.Quantity)
            .IsRequired();

        builder.Property(l => l.SnapshotTotalQuantity)
            .IsRequired();

        builder.Property(l => l.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Nullable OrderId for manual import/export operations
        builder.Property(l => l.OrderId)
            .IsRequired(false);

        // Relationships
        builder.HasOne(l => l.StockItem)
            .WithMany() // StockLog không có navigation property ngược lại
            .HasForeignKey(l => l.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t => {
            t.HasCheckConstraint("CK_StockLog_Quantity", "\"Quantity\" > 0");
            t.HasCheckConstraint("CK_StockLog_SnapshotTotalQuantity", "\"SnapshotTotalQuantity\" >= 0");
        });

        // Indexes for performance
        builder.HasIndex(l => l.StockItemId);
        builder.HasIndex(l => l.OrderId)
            .HasFilter("\"OrderId\" IS NOT NULL"); // Partial index for non-null OrderId
        builder.HasIndex(l => l.Type);
        builder.HasIndex(l => new { l.StockItemId, l.CreatedAt })
            .HasDatabaseName("IX_StockLog_StockItemId_CreatedAt");
        builder.HasIndex(l => new { l.OrderId, l.CreatedAt })
            .HasDatabaseName("IX_StockLog_OrderId_CreatedAt")
            .HasFilter("\"OrderId\" IS NOT NULL");

        // Table configuration
        builder.ToTable("StockLogs");
    }
}