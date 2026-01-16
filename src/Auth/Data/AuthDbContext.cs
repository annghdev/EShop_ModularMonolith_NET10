using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace Auth.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) 
    : IdentityDbContext<Account, Role, Guid>(options)
{
    public DbSet<ExternalAccount> ExternalAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply configurations from assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
