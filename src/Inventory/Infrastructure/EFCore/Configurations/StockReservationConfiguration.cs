using Inventory.Domain;
using Kernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EFCore.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        // Base entity configuration
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.OrderId)
            .IsRequired();

        builder.Property(r => r.Quantity)
            .IsRequired();

        // Relationships
        builder.HasOne(r => r.StockItem)
            .WithMany(s => s.Reservations)
            .HasForeignKey(r => r.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t => {
            t.HasCheckConstraint("CK_StockReservation_Quantity", "\"Quantity\" > 0");
        });

        // Unique constraint: One reservation per order per stock item
        builder.HasIndex(r => new { r.OrderId, r.StockItemId })
            .IsUnique()
            .HasDatabaseName("IX_StockReservation_OrderId_StockItemId_Unique");

        // Indexes for performance
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.CreatedAt);
        builder.HasIndex(r => new { r.StockItemId, r.CreatedAt });
    }
}