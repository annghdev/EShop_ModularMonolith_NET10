using Inventory.Domain;
using Kernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EFCore.Configurations;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        new AggreageRootConfiguration<StockItem>().Configure(builder);

        // Properties configuration
        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Quantity)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.ThresholdWarning)
            .HasDefaultValue(5)
            .IsRequired();

        // Map Sku ValueObject
        builder.OwnsOne(s => s.Sku, sku =>
        {
            sku.Property(s => s.Value)
                .HasColumnName("Sku")
                .HasMaxLength(50)
                .IsRequired();

            sku.HasIndex(s => s.Value)
                .IsUnique();
        });

        // Relationships
        builder.HasMany(s => s.Reservations)
            .WithOne(r => r.StockItem)
            .HasForeignKey(r => r.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t => {
            t.HasCheckConstraint("CK_StockItem_Quantity", "\"Quantity\" >= 0");
            t.HasCheckConstraint("CK_StockItem_ThresholdWarning", "\"ThresholdWarning\" >= 0");
        });

        // Indexes for performance
        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.Quantity);
    }
}
