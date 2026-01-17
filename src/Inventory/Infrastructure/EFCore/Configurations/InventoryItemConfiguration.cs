using Inventory.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EFCore.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        // Use aggregate root configuration for base properties
        new AggreageRootConfiguration<InventoryItem>().Configure(builder);

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.VariantId)
            .IsRequired();

        // Map Sku ValueObject
        builder.OwnsOne(i => i.Sku, sku =>
        {
            sku.Property(s => s.Value)
                .HasColumnName("Sku")
                .HasMaxLength(50)
                .IsRequired();

            sku.HasIndex(s => s.Value)
                .HasDatabaseName("IX_InventoryItems_Sku");
        });

        builder.Property(i => i.QuantityOnHand)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(i => i.LowStockThreshold)
            .HasDefaultValue(5)
            .IsRequired();

        // Relationships
        builder.HasOne(i => i.Warehouse)
            .WithMany(w => w.InventoryItems)
            .HasForeignKey(i => i.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Reservations)
            .WithOne(r => r.InventoryItem)
            .HasForeignKey(r => r.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InventoryItems_QuantityOnHand", "\"QuantityOnHand\" >= 0");
            t.HasCheckConstraint("CK_InventoryItems_LowStockThreshold", "\"LowStockThreshold\" >= 0");
        });

        // Indexes
        builder.HasIndex(i => new { i.WarehouseId, i.VariantId })
            .IsUnique()
            .HasDatabaseName("IX_InventoryItems_WarehouseId_VariantId_Unique");

        builder.HasIndex(i => i.ProductId)
            .HasDatabaseName("IX_InventoryItems_ProductId");

        builder.HasIndex(i => i.VariantId)
            .HasDatabaseName("IX_InventoryItems_VariantId");

        builder.HasIndex(i => new { i.WarehouseId, i.QuantityOnHand })
            .HasDatabaseName("IX_InventoryItems_WarehouseId_QuantityOnHand");

        builder.ToTable("InventoryItems");
    }
}
