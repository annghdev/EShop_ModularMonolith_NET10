using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoppingCart.Domain;

namespace ShoppingCart.Infrastructure.EFCore.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        new AggreageRootConfiguration<Cart>().Configure(builder);

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.AppliedCouponCode)
            .HasMaxLength(50);

        builder.Property(c => c.AppliedCouponId)
            .IsRequired(false);

        builder.Property(c => c.LastActivityAt)
            .IsRequired(false);

        #region Map ValueObjects

        builder.OwnsOne(c => c.Customer, customer =>
        {
            customer.Property(x => x.CustomerId)
                .HasColumnName("CustomerId");

            customer.Property(x => x.GuestId)
                .HasColumnName("GuestId")
                .HasMaxLength(100);

            customer.HasIndex(x => x.CustomerId);
            customer.HasIndex(x => x.GuestId);
        });

        builder.OwnsOne(c => c.SubTotal, money =>
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

        builder.OwnsOne(c => c.TotalDiscount, money =>
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

        builder.OwnsOne(c => c.EstimatedTotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("EstimatedTotal")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("EstimatedTotalCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        #endregion

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Cart.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.LastActivityAt);

        builder.ToTable("Carts");
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
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

        builder.Property(i => i.DiscountDescription)
            .HasMaxLength(300);

        builder.Property(i => i.AppliedPromotionId)
            .IsRequired(false);

        builder.Property(i => i.Quantity)
            .IsRequired();

        #region Map ValueObjects

        builder.OwnsOne(i => i.OriginalPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("OriginalPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("OriginalPriceCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

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

        builder.HasIndex(i => i.CartId);
        builder.HasIndex(i => i.VariantId);

        builder.ToTable("CartItems");
    }
}
