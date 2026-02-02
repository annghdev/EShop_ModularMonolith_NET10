using Catalog.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Attribute = Catalog.Domain.Attribute;

namespace Catalog.Infrastructure.EFCore.Configurations;

public class AttributeConfiguration : IEntityTypeConfiguration<Attribute>
{
    public void Configure(EntityTypeBuilder<Attribute> builder)
    {
        new AggreageRootConfiguration<Attribute>().Configure(builder);

        builder.Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Icon)
            .HasMaxLength(500);

        builder.Property(a => a.ValueStyleCss)
            .HasMaxLength(1000);

        builder.HasMany(a => a.Values)
            .WithOne(av => av.Attribute)
            .HasForeignKey(av => av.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
