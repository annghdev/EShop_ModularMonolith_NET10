using Catalog.Domain;
using Kernel.Infrastructure.EFCore;
using System.Reflection;
using Attribute = Catalog.Domain.Attribute;

namespace Catalog.Infrastructure;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : BaseDbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Variant> Variants { get; set; }
    public DbSet<Attribute> Attributes { get; set; }
    public DbSet<AttributeValue> AttributeValues { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<VariantAttributeValue> VariantAttributeValues { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Collection> Collections { get; set; }

    protected override Assembly GetAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
