using Catalog.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.EFCore.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        new AggreageRootConfiguration<Category>().Configure(builder);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Image)
            .HasMaxLength(500);

        #region Hierarchical Relationship

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        builder.HasMany(c => c.DefaultAttributes)
            .WithOne(cda => cda.Category)
            .HasForeignKey(cda => cda.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Name)
            .IsUnique();
    }
}

public class CategoryDefaultAttributeConfiguration : IEntityTypeConfiguration<CategoryDefaultAttribute>
{
    public void Configure(EntityTypeBuilder<CategoryDefaultAttribute> builder)
    {
        builder.HasOne(cda => cda.Attribute)
            .WithMany()
            .HasForeignKey(cda => cda.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cda => cda.Category)
            .WithMany(c => c.DefaultAttributes)
            .HasForeignKey(cda => cda.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cda => new { cda.CategoryId, cda.AttributeId })
            .IsUnique();
    }
}
