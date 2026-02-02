using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain;

namespace Orders.Infrastructure.EFCore.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        new AggreageRootConfiguration<Order>().Configure(builder);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.PaymentMethod)
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .IsRequired();

        builder.Property(o => o.PaymentId)
            .IsRequired(false);

        builder.Property(o => o.PaidAt)
            .IsRequired(false);

        builder.Property(o => o.ShippingMethod)
            .IsRequired();

        builder.Property(o => o.ShippingId)
            .IsRequired(false);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.CustomerNote)
            .HasMaxLength(1000);

        builder.Property(o => o.AdminNote)
            .HasMaxLength(1000);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(1000);

        #region Map ValueObjects

        builder.OwnsOne(o => o.ShippingAddress, address =>
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

        builder.OwnsOne(o => o.BillingAddress, address =>
        {
            address.Property(a => a.RecipientName)
                .HasColumnName("BillingRecipientName")
                .HasMaxLength(200)
                .IsRequired(false);

            address.Property(a => a.Phone)
                .HasColumnName("BillingPhone")
                .HasMaxLength(20)
                .IsRequired(false);

            address.Property(a => a.Street)
                .HasColumnName("BillingStreet")
                .HasMaxLength(300)
                .IsRequired(false);

            address.Property(a => a.Ward)
                .HasColumnName("BillingWard")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(a => a.District)
                .HasColumnName("BillingDistrict")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(a => a.City)
                .HasColumnName("BillingCity")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(a => a.Country)
                .HasColumnName("BillingCountry")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(a => a.PostalCode)
                .HasColumnName("BillingPostalCode")
                .HasMaxLength(20)
                .IsRequired(false);
        });

        builder.Navigation(o => o.BillingAddress)
            .IsRequired(false);

        builder.OwnsOne(o => o.SubTotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("SubTotal")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("SubTotalCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.OwnsOne(o => o.ShippingFee, money =>
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

        builder.OwnsOne(o => o.TotalDiscount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalDiscount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TotalDiscountCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.OwnsOne(o => o.GrandTotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("GrandTotal")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("GrandTotalCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        #endregion

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Discounts)
            .WithOne(d => d.Order)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Order.Discounts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Order.StatusHistory))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PaymentStatus);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);

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
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Thumbnail)
            .HasMaxLength(500);

        builder.Property(i => i.Quantity)
            .IsRequired();

        #region Map ValueObjects

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.OwnsOne(i => i.DiscountAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("DiscountAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("DiscountCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        #endregion

        builder.HasIndex(i => i.OrderId);
        builder.HasIndex(i => i.VariantId);
    }
}

public class OrderDiscountConfiguration : IEntityTypeConfiguration<OrderDiscount>
{
    public void Configure(EntityTypeBuilder<OrderDiscount> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Source)
            .IsRequired();

        builder.Property(d => d.SourceId)
            .IsRequired(false);

        builder.Property(d => d.SourceCode)
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .HasMaxLength(500)
            .IsRequired();

        #region Map ValueObjects

        builder.OwnsOne(d => d.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("AmountCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        #endregion

        builder.HasIndex(d => d.OrderId);
        builder.HasIndex(d => d.Source);
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FromStatus)
            .IsRequired();

        builder.Property(s => s.ToStatus)
            .IsRequired();

        builder.Property(s => s.Reason)
            .HasMaxLength(1000);

        builder.Property(s => s.ChangedBy)
            .IsRequired(false);

        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.ToStatus);
    }
}
