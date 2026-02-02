using Catalog.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.EFCore.Configurations;

public class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        #region Relationship

        builder.HasOne(vav => vav.Variant)
            .WithMany(v => v.AttributeValues)
            .HasForeignKey(vav => vav.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vav => vav.Value)
            .WithMany()
            .HasForeignKey(vav => vav.ValueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(vav => vav.ProductAttribute)
            .WithMany()
            .HasForeignKey(vav => vav.ProductAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        builder.HasIndex(vav => new { vav.VariantId, vav.ProductAttributeId })
            .IsUnique();
    }
}
