using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;

/// <summary>
/// Design-time factory for InfrasDbContext.
/// This allows EF Core migrations to work without starting the full application.
/// </summary>
public class DesignTimeInfrasDbContextFactory : IDesignTimeDbContextFactory<InfrasDbContext>
{
    public InfrasDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InfrasDbContext>();
        
        // Default connection string for design-time migrations
        // This will be overridden at runtime by the actual configuration
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=infrasdb;Username=postgres;Password=postgres");

        return new InfrasDbContext(optionsBuilder.Options);
    }
}
