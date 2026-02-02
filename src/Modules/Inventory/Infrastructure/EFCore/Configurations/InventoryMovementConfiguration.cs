using Inventory.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EFCore.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.InventoryItemId)
            .IsRequired();

        builder.Property(m => m.WarehouseId)
            .IsRequired();

        builder.Property(m => m.ProductId)
            .IsRequired();

        builder.Property(m => m.VariantId)
            .IsRequired();

        builder.Property(m => m.OrderId)
            .IsRequired(false);

        builder.Property(m => m.Quantity)
            .IsRequired();

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.SnapshotQuantity)
            .IsRequired();

        builder.Property(m => m.Reference)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(m => m.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        // Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InventoryMovements_Quantity", "\"Quantity\" > 0");
            t.HasCheckConstraint("CK_InventoryMovements_SnapshotQuantity", "\"SnapshotQuantity\" >= 0");
        });

        // Indexes for audit queries
        builder.HasIndex(m => m.InventoryItemId)
            .HasDatabaseName("IX_InventoryMovements_InventoryItemId");

        builder.HasIndex(m => m.WarehouseId)
            .HasDatabaseName("IX_InventoryMovements_WarehouseId");

        builder.HasIndex(m => m.ProductId)
            .HasDatabaseName("IX_InventoryMovements_ProductId");

        builder.HasIndex(m => m.VariantId)
            .HasDatabaseName("IX_InventoryMovements_VariantId");

        builder.HasIndex(m => m.OrderId)
            .HasFilter("\"OrderId\" IS NOT NULL")
            .HasDatabaseName("IX_InventoryMovements_OrderId");

        builder.HasIndex(m => m.Type)
            .HasDatabaseName("IX_InventoryMovements_Type");

        builder.HasIndex(m => new { m.InventoryItemId, m.CreatedAt })
            .HasDatabaseName("IX_InventoryMovements_InventoryItemId_CreatedAt");

        builder.HasIndex(m => new { m.WarehouseId, m.CreatedAt })
            .HasDatabaseName("IX_InventoryMovements_WarehouseId_CreatedAt");

        builder.HasIndex(m => new { m.OrderId, m.CreatedAt })
            .HasFilter("\"OrderId\" IS NOT NULL")
            .HasDatabaseName("IX_InventoryMovements_OrderId_CreatedAt");

        builder.ToTable("InventoryMovements");
    }
}
