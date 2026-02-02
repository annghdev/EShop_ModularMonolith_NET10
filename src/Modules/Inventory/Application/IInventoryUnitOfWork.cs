using Inventory.Domain;
using Kernel.Application.Interfaces;

namespace Inventory.Application;

public interface IInventoryUnitOfWork : IUnitOfWork
{
    DbSet<Warehouse> Warehouses { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<InventoryMovement> InventoryMovements { get; }
    IWarehouseRepository WarehouseRepository { get; }
    IInventoryItemRepository InventoryItemRepository { get; }
}
