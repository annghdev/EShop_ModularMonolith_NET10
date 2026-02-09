using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.Infrastructure;

/// <summary>
/// Factory for creating InventoryDbContext during design-time operations (migrations, etc.)
/// </summary>
public class DesignTimeInventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        
        // Default connection string for design-time operations
        // This is only used when running EF Core CLI commands
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=eshop_inventory;Username=postgres;Password=postgres");
        
        return new InventoryDbContext(optionsBuilder.Options);
    }
}
