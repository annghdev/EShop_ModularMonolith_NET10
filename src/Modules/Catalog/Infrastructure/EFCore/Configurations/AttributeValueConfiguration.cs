using Catalog.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.EFCore.Configurations;

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.Property(av => av.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(av => av.ColorCode)
            .HasMaxLength(7); // Hex color code format #RRGGBB

        builder.HasOne(av => av.Attribute)
            .WithMany(a => a.Values)
            .HasForeignKey(av => av.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
