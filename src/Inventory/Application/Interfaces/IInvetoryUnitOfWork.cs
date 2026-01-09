using Inventory.Domain;
using Kernel.Application.Interfaces;

namespace Inventory.Application;

public interface IInventoryUnitOfWork : IUnitOfWork
{
    DbSet<StockItem> StockItems { get; }
    DbSet<StockLog> StockLogs { get; }
}
