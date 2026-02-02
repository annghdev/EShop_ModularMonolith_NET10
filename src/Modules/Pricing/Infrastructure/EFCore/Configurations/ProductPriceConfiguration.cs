using Pricing.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pricing.Infrastructure.EFCore.Configurations;

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        new AggreageRootConfiguration<ProductPrice>().Configure(builder);

        builder.Property(p => p.ProductId)
            .IsRequired();

        builder.Property(p => p.VariantId)
            .IsRequired(false);

        builder.HasIndex(p => p.ProductId);
        builder.HasIndex(p => p.VariantId);

        #region Map ValueObjects

        builder.OwnsOne(p => p.Sku, sku =>
        {
            sku.Property(s => s.Value)
                .HasColumnName("Sku")
                .HasMaxLength(50)
                .IsRequired();

            sku.HasIndex(s => s.Value)
                .IsUnique();
        });

        builder.OwnsOne(p => p.CurrentCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CurrentCost")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("CurrentCostCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.OwnsOne(p => p.CurrentPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CurrentPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("CurrentPriceCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        #endregion

        builder.HasMany(p => p.ChangeLogs)
            .WithOne(c => c.ProductPrice)
            .HasForeignKey(c => c.ProductPriceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(ProductPrice.ChangeLogs))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("ProductPrices");
    }
}

public class PriceChangeLogConfiguration : IEntityTypeConfiguration<PriceChangeLog>
{
    public void Configure(EntityTypeBuilder<PriceChangeLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ChangeType)
            .IsRequired();

        builder.Property(l => l.Reason)
            .HasMaxLength(500);

        builder.Property(l => l.ChangedBy)
            .IsRequired(false);

        #region Map ValueObjects

        builder.OwnsOne(l => l.PreviousCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PreviousCost")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("PreviousCostCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.OwnsOne(l => l.NewCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("NewCost")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("NewCostCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.OwnsOne(l => l.PreviousPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PreviousPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("PreviousPriceCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        builder.OwnsOne(l => l.NewPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("NewPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("NewPriceCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();
        });

        #endregion

        builder.HasIndex(l => l.ProductPriceId);
        builder.HasIndex(l => new { l.ProductPriceId, l.CreatedAt });

        builder.ToTable("PriceChangeLogs");
    }
}
