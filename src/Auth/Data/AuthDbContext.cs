using Auth.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Auth.Data;

public class AuthDbContext : IdentityDbContext<Account, Role, Guid>
{
    public DbSet<ExternalAccount> ExternalAccounts { get; set; }
}
