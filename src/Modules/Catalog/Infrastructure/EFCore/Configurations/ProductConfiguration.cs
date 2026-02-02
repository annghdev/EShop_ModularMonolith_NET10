using Catalog.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.EFCore.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        new AggreageRootConfiguration<Product>().Configure(builder);

        builder.Property(p=>p.Name).HasMaxLength(200);

        #region Map ValueObjects

        builder.OwnsOne(p => p.Slug, slug =>
        {
            slug.Property(m => m.Value)
                 .HasColumnName("Slug")
                 .HasMaxLength(200)
                 .IsRequired();
        });

        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("Price")
                 .HasPrecision(18, 2)
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("PriceCurrency")
                 .HasMaxLength(3)
                 .HasDefaultValue("VND")
                 .IsRequired();
        });

        builder.OwnsOne(p => p.Cost, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("Cost")
                 .HasPrecision(18, 2)
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("CostCurrency")
                 .HasMaxLength(3)
                 .HasDefaultValue("VND")
                 .IsRequired();
        });

        builder.OwnsOne(p => p.Dimensions, dim =>
        {
            dim.Property(d => d.Width)
               .HasColumnName("Width")
               .HasPrecision(18, 2);

            dim.Property(d => d.Height)
               .HasColumnName("Height")
               .HasPrecision(18, 2);

            dim.Property(d => d.Depth)
               .HasColumnName("Depth")
               .HasPrecision(18, 2);

            dim.Property(d => d.Weight)
               .HasColumnName("Weight")
               .HasPrecision(18, 2);
        });

        builder.OwnsOne(p => p.Thumbnail, dim =>
        {
            dim.Property(d => d.Path)
               .HasColumnName("Thumbnail");
        });

        builder.OwnsMany(p => p.Images, img =>
        {
            img.ToTable("ProductImages");

            img.WithOwner()
               .HasForeignKey("ProductId");

            img.Property<Guid>("Id"); // Shadow key cho EF
            img.HasKey("Id");

            img.Property(i => i.Path)
               .HasColumnName("ImageUrl")
               .IsRequired()
               .HasMaxLength(500);
        });

        // Quan trọng: map backing field
        builder.Metadata
            .FindNavigation(nameof(Product.Images))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        #endregion

        #region Relationship

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        #endregion
    }
}
