using Kernel.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure;

public abstract class BaseDbContext(DbContextOptions options) : DbContext(options)
{
    protected abstract Assembly GetAssembly();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplySoftDeleteFilter();
        modelBuilder.ApplyConfigurationsFromAssembly(GetAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
