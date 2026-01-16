using System.Reflection;

namespace Users.Infrastructure;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : BaseDbContext(options)
{
    protected override Assembly GetAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}
