using Catalog.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.EFCore.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        #region Relationship

        builder.HasOne(pa => pa.Product)
            .WithMany(p => p.Attributes)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.Attribute)
            .WithMany()
            .HasForeignKey(pa => pa.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        builder.HasIndex(pa => new { pa.ProductId, pa.AttributeId })
            .IsUnique();
    }
}
