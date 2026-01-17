using Inventory.Domain;
using System.Reflection;

namespace Inventory.Infrastructure;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) 
    : BaseDbContext(options)
{
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<InventoryReservation> InventoryReservations { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }

    protected override Assembly GetAssembly()
    {
        return typeof(InventoryDbContext).Assembly;
    }
}
