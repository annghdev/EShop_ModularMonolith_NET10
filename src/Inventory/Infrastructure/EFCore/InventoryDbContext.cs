using Inventory.Domain;
using System.Reflection;

namespace Inventory.Infrastructure;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) 
    : BaseDbContext(options)
{
    public DbSet<StockItem> StockItems { get; set; }
    public DbSet<StockReservation> StockReservations { get; set; }
    public DbSet<StockLog> StockLogs { get; set; }

    protected override Assembly GetAssembly()
    {
        return typeof(InventoryDbContext).Assembly;
    }
}
