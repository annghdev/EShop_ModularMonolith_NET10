using Catalog.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.EFCore.Configurations;

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.Property(v => v.Name).HasMaxLength(200);

        #region Map ValueObjects

        builder.OwnsOne(v => v.Sku, sku =>
        {
            sku.Property(m => m.Value)
                 .HasColumnName("Sku")
                 .HasMaxLength(50)
                 .IsRequired();
        });

        builder.OwnsOne(v => v.OverridePrice, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("OverridePrice")
                 .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                 .HasColumnName("OverridePriceCurrency")
                 .HasMaxLength(3)
                 .HasDefaultValue("VND");
        });

        builder.OwnsOne(v => v.OverrideCost, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("OverrideCost")
                 .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                 .HasColumnName("OverrideCostCurrency")
                 .HasMaxLength(3)
                 .HasDefaultValue("VND");
        });

        builder.OwnsOne(v => v.OverrideDimensions, dim =>
        {
            dim.Property(d => d.Width)
               .HasColumnName("OverrideWidth")
               .HasPrecision(18, 2);

            dim.Property(d => d.Height)
               .HasColumnName("OverrideHeight")
               .HasPrecision(18, 2);

            dim.Property(d => d.Depth)
               .HasColumnName("OverrideDepth")
               .HasPrecision(18, 2);

            dim.Property(d => d.Weight)
               .HasColumnName("OverrideWeight")
               .HasPrecision(18, 2);
        });

        builder.OwnsOne(v => v.MainImage, img =>
        {
            img.Property(i => i.Path)
               .HasColumnName("MainImage");
        });

        builder.OwnsMany(v => v.Images, img =>
        {
            img.ToTable("VariantImages");

            img.WithOwner()
               .HasForeignKey("VariantId");

            img.Property<Guid>("Id"); // Shadow key cho EF
            img.HasKey("Id");

            img.Property(i => i.Path)
               .HasColumnName("ImageUrl")
               .IsRequired()
               .HasMaxLength(500);
        });

        // Quan trọng: map backing field
        builder.Metadata
            .FindNavigation(nameof(Variant.Images))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        #endregion

        #region Relationship

        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasMany(v => v.AttributeValues)
            .WithOne(vav => vav.Variant)
            .HasForeignKey(vav => vav.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
