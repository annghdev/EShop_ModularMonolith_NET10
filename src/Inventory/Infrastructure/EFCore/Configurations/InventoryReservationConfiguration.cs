using Inventory.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EFCore.Configurations;

public class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.OrderId)
            .IsRequired();

        builder.Property(r => r.Quantity)
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .IsRequired(false);

        // Relationships
        builder.HasOne(r => r.InventoryItem)
            .WithMany(i => i.Reservations)
            .HasForeignKey(r => r.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InventoryReservations_Quantity", "\"Quantity\" > 0");
        });

        // Unique constraint: One reservation per order per inventory item
        builder.HasIndex(r => new { r.InventoryItemId, r.OrderId })
            .IsUnique()
            .HasDatabaseName("IX_InventoryReservations_InventoryItemId_OrderId_Unique");

        // Indexes for queries
        builder.HasIndex(r => r.OrderId)
            .HasDatabaseName("IX_InventoryReservations_OrderId");

        builder.HasIndex(r => r.ExpiresAt)
            .HasFilter("\"ExpiresAt\" IS NOT NULL")
            .HasDatabaseName("IX_InventoryReservations_ExpiresAt");

        builder.HasIndex(r => r.CreatedAt)
            .HasDatabaseName("IX_InventoryReservations_CreatedAt");

        builder.ToTable("InventoryReservations");
    }
}
