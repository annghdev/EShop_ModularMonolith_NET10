using Pricing.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pricing.Infrastructure.EFCore.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        new AggreageRootConfiguration<Promotion>().Configure(builder);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Type)
            .IsRequired();

        builder.Property(p => p.Priority)
            .IsRequired();

        builder.Property(p => p.IsStackable)
            .IsRequired();

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.EndDate);

        builder.HasMany(p => p.Rules)
            .WithOne(r => r.Promotion)
            .HasForeignKey(r => r.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Actions)
            .WithOne(a => a.Promotion)
            .HasForeignKey(a => a.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Promotion.Rules))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Promotion.Actions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("Promotions");
    }
}

public class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .IsRequired();

        builder.Property(r => r.TargetId)
            .IsRequired(false);

        builder.Property(r => r.MinQuantity)
            .IsRequired(false);

        builder.Property(r => r.CustomerTier)
            .HasMaxLength(100);

        #region Map ValueObjects

        builder.OwnsOne(r => r.MinOrderValue, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("MinOrderValue")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("MinOrderCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND");
        });

        #endregion

        builder.Navigation(r => r.MinOrderValue)
            .IsRequired(false);

        builder.HasIndex(r => r.PromotionId);
        builder.HasIndex(r => r.Type);

        builder.ToTable("PromotionRules");
    }
}

public class PromotionActionConfiguration : IEntityTypeConfiguration<PromotionAction>
{
    public void Configure(EntityTypeBuilder<PromotionAction> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.GiftProductId)
            .IsRequired(false);

        builder.Property(a => a.GiftVariantId)
            .IsRequired(false);

        builder.Property(a => a.GiftQuantity)
            .IsRequired();

        #region Map ValueObjects

        builder.OwnsOne(a => a.Discount, discount =>
        {
            discount.Property(d => d.Type)
                .HasColumnName("DiscountType");

            discount.Property(d => d.Amount)
                .HasColumnName("DiscountAmount")
                .HasPrecision(18, 2);
        });

        #endregion

        builder.HasIndex(a => a.PromotionId);
        builder.HasIndex(a => a.Type);

        builder.Navigation(a => a.Discount)
            .IsRequired(false);

        builder.ToTable("PromotionActions");
    }
}
