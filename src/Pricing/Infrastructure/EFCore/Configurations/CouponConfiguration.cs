using Pricing.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pricing.Infrastructure.EFCore.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        new AggreageRootConfiguration<Coupon>().Configure(builder);

        builder.Property(c => c.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();

        builder.Property(c => c.UsageLimitTotal)
            .IsRequired(false);

        builder.Property(c => c.UsageLimitPerCustomer)
            .IsRequired(false);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.EndDate);

        #region Map ValueObjects

        builder.OwnsOne(c => c.Discount, discount =>
        {
            discount.Property(d => d.Type)
                .HasColumnName("DiscountType")
                .IsRequired();

            discount.Property(d => d.Amount)
                .HasColumnName("DiscountAmount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.OwnsOne(c => c.MinOrderValue, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("MinOrderValue")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("MinOrderCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND");
        });

        builder.OwnsOne(c => c.MaxDiscountAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("MaxDiscountAmount")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("MaxDiscountCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND");
        });

        #endregion

        builder.HasMany(c => c.Conditions)
            .WithOne(cc => cc.Coupon)
            .HasForeignKey(cc => cc.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Usages)
            .WithOne(cu => cu.Coupon)
            .HasForeignKey(cu => cu.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Coupon.Conditions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Coupon.Usages))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(c => c.MinOrderValue)
            .IsRequired(false);

        builder.Navigation(c => c.MaxDiscountAmount)
            .IsRequired(false);

        builder.ToTable("Coupons");
    }
}

public class CouponConditionConfiguration : IEntityTypeConfiguration<CouponCondition>
{
    public void Configure(EntityTypeBuilder<CouponCondition> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.TargetId)
            .IsRequired(false);

        builder.Property(c => c.MinQuantity)
            .IsRequired(false);

        builder.HasIndex(c => c.CouponId);
        builder.HasIndex(c => c.Type);

        builder.ToTable("CouponConditions");
    }
}

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.CustomerId)
            .IsRequired();

        builder.Property(u => u.OrderId)
            .IsRequired();

        builder.Property(u => u.UsedAt)
            .IsRequired();

        #region Map ValueObjects

        builder.OwnsOne(u => u.DiscountApplied, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("DiscountApplied")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("DiscountAppliedCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        #endregion

        builder.HasIndex(u => u.CouponId);
        builder.HasIndex(u => u.CustomerId);
        builder.HasIndex(u => u.OrderId);
        builder.HasIndex(u => u.UsedAt);

        builder.ToTable("CouponUsages");
    }
}
