using System.Reflection;
using Users.Domain;

namespace Users.Infrastructure;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : BaseDbContext(options)
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Guest> Guests { get; set; }

    protected override Assembly GetAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}
