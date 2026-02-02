using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Domain;

namespace Shipping.Infrastructure.EFCore.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        new AggreageRootConfiguration<Shipment>().Configure(builder);

        builder.Property(s => s.ShipmentNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.OrderId)
            .IsRequired();

        builder.Property(s => s.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.EstimatedDeliveryDate);

        builder.Property(s => s.ActualDeliveryDate);

        builder.Property(s => s.Note)
            .HasMaxLength(1000);

        builder.Property(s => s.FailureReason)
            .HasMaxLength(1000);

        #region Map ValueObjects

        builder.OwnsOne(s => s.ShippingAddress, address =>
        {
            address.Property(a => a.RecipientName)
                .HasColumnName("ShippingRecipientName")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.Phone)
                .HasColumnName("ShippingPhone")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .HasMaxLength(300)
                .IsRequired();

            address.Property(a => a.Ward)
                .HasColumnName("ShippingWard")
                .HasMaxLength(100);

            address.Property(a => a.District)
                .HasColumnName("ShippingDistrict")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("ShippingPostalCode")
                .HasMaxLength(20);
        });

        builder.OwnsOne(s => s.ShippingFee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ShippingFee")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("ShippingFeeCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.OwnsOne(s => s.CODAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CODAmount")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("CODCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND");
        });

        builder.OwnsOne(s => s.Package, package =>
        {
            package.Property(p => p.WeightInGrams)
                .HasColumnName("PackageWeightInGrams")
                .HasPrecision(18, 2)
                .IsRequired();

            package.Property(p => p.LengthInCm)
                .HasColumnName("PackageLengthInCm")
                .HasPrecision(18, 2)
                .IsRequired();

            package.Property(p => p.WidthInCm)
                .HasColumnName("PackageWidthInCm")
                .HasPrecision(18, 2)
                .IsRequired();

            package.Property(p => p.HeightInCm)
                .HasColumnName("PackageHeightInCm")
                .HasPrecision(18, 2)
                .IsRequired();

            package.Property(p => p.ItemCount)
                .HasColumnName("PackageItemCount")
                .IsRequired();
        });

        builder.Navigation(s => s.CODAmount)
            .IsRequired(false);

        #endregion

        #region Relationships

        builder.HasOne(s => s.Carrier)
            .WithMany()
            .HasForeignKey(s => s.CarrierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.TrackingEvents)
            .WithOne()
            .HasForeignKey(e => e.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Shipment.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Shipment.TrackingEvents))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        #endregion

        #region Indexes

        builder.HasIndex(s => s.ShipmentNumber)
            .IsUnique();

        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.OrderNumber);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.TrackingNumber);

        #endregion
    }
}
