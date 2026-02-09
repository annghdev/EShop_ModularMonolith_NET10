using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infrastructure;

/// <summary>
/// Factory for creating CatalogDbContext during design-time operations (migrations, etc.)
/// </summary>
public class DesignTimeCatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        
        // Default connection string for design-time operations
        // This is only used when running EF Core CLI commands
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=eshop_catalog;Username=postgres;Password=postgres");
        
        return new CatalogDbContext(optionsBuilder.Options);
    }
}
