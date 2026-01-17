using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Domain;

namespace Shipping.Infrastructure.EFCore.Configurations;

public class ShippingCarrierConfiguration : IEntityTypeConfiguration<ShippingCarrier>
{
    public void Configure(EntityTypeBuilder<ShippingCarrier> builder)
    {
        new AggreageRootConfiguration<ShippingCarrier>().Configure(builder);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IsEnabled)
            .IsRequired();

        builder.Property(c => c.DisplayOrder)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property<List<string>>("_supportedRegions")
            .HasColumnName("SupportedRegions")
            .HasColumnType("jsonb");

        builder.Ignore(c => c.SupportedRegions);

        builder.HasOne(c => c.Setting)
            .WithOne()
            .HasForeignKey<ShippingCarrierSetting>("ShippingCarrierId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.HasIndex(c => c.Provider);
        builder.HasIndex(c => c.IsEnabled);
    }
}
