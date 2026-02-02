using Kernel.Application;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class InfrasDbContext(DbContextOptions<InfrasDbContext> options) : DbContext(options)
{
    public DbSet<FileMetadata> FileMetadata { get; set; }
}
