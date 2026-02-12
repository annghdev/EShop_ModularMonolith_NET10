using Microsoft.EntityFrameworkCore.Design;

namespace Auth.Data;

public class DesignTimeAuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=auth_design_time;Username=postgres;Password=postgres");
        return new AuthDbContext(optionsBuilder.Options);
    }
}
