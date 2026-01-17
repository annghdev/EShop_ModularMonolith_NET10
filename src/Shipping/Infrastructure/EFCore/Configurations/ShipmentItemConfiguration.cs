using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Domain;

namespace Shipping.Infrastructure.EFCore.Configurations;

public class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ShipmentId)
            .IsRequired();

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.VariantId)
            .IsRequired();

        builder.Property(i => i.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.VariantName)
            .HasMaxLength(200);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.HasIndex(i => i.ShipmentId);
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.VariantId);
    }
}
