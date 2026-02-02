using Inventory.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EFCore.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        new AggreageRootConfiguration<Warehouse>().Configure(builder);

        builder.Property(w => w.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(w => w.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(w => w.IsDefault)
            .HasDefaultValue(false)
            .IsRequired();

        // Address as owned type
        builder.OwnsOne(w => w.Address, address =>
        {
            address.Property(a => a.RecipientName)
                .HasColumnName("Address_RecipientName")
                .HasMaxLength(200);

            address.Property(a => a.Phone)
                .HasColumnName("Address_Phone")
                .HasMaxLength(50);

            address.Property(a => a.Street)
                .HasColumnName("Address_Street")
                .HasMaxLength(500);

            address.Property(a => a.Ward)
                .HasColumnName("Address_Ward")
                .HasMaxLength(100);

            address.Property(a => a.District)
                .HasColumnName("Address_District")
                .HasMaxLength(100);

            address.Property(a => a.City)
                .HasColumnName("Address_City")
                .HasMaxLength(100);

            address.Property(a => a.Country)
                .HasColumnName("Address_Country")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("Address_PostalCode")
                .HasMaxLength(20);
        });

        // Relationships
        builder.HasMany(w => w.InventoryItems)
            .WithOne(i => i.Warehouse)
            .HasForeignKey(i => i.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(w => w.Code)
            .IsUnique()
            .HasDatabaseName("IX_Warehouses_Code_Unique");

        builder.HasIndex(w => w.IsActive)
            .HasDatabaseName("IX_Warehouses_IsActive");

        builder.HasIndex(w => w.IsDefault)
            .HasFilter("\"IsDefault\" = true")
            .HasDatabaseName("IX_Warehouses_IsDefault");

        builder.ToTable("Warehouses");
    }
}
