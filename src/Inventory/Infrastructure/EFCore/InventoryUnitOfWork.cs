using Inventory.Application;
using Inventory.Domain;

namespace Inventory.Infrastructure;

public class InventoryUnitOfWork(
    InventoryDbContext context,
    ICurrentUser user,
    IPublisher publisher,
    IWarehouseRepository warehouseRepository,
    IInventoryItemRepository inventoryItemRepository)
    : BaseUnitOfWork<InventoryDbContext>(context, user, publisher), IInventoryUnitOfWork
{
    public DbSet<Warehouse> Warehouses => context.Warehouses;
    public DbSet<InventoryItem> InventoryItems => context.InventoryItems;
    public DbSet<InventoryMovement> InventoryMovements => context.InventoryMovements;
    public IWarehouseRepository WarehouseRepository => warehouseRepository;
    public IInventoryItemRepository InventoryItemRepository => inventoryItemRepository;
}
