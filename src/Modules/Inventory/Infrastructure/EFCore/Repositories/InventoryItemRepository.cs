using Inventory.Domain;

namespace Inventory.Infrastructure;

public class InventoryItemRepository(InventoryDbContext context)
    : BaseRepository<InventoryItem, InventoryDbContext>(context), IInventoryItemRepository
{
    public override async Task<InventoryItem> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = context.InventoryItems
            .Include(i => i.Reservations)
            .AsQueryable();

        if (!changeTracking)
            query = query.AsNoTracking();

        return await query.FirstAsync(i => i.Id == id);
    }

    public override async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.InventoryItems
            .Include(i => i.Reservations)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryItem?> GetByVariantIdAsync(Guid variantId, Guid? warehouseId = null, CancellationToken cancellationToken = default)
    {
        var query = context.InventoryItems
            .Include(i => i.Reservations)
            .Include(i => i.Warehouse)
            .Where(i => i.VariantId == variantId);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }
        else
        {
            // If no warehouse specified, prefer default warehouse or first available
            query = query.OrderByDescending(i => i.Warehouse!.IsDefault);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryItem>> GetAllByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryItem>> GetAllByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.VariantId == variantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryItem?> GetBySkuAsync(string sku, Guid? warehouseId = null, CancellationToken cancellationToken = default)
    {
        var query = context.InventoryItems
            .Include(i => i.Reservations)
            .Include(i => i.Warehouse)
            .Where(i => i.Sku.Value == sku);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }
        else
        {
            query = query.OrderByDescending(i => i.Warehouse!.IsDefault);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(Guid? warehouseId = null, CancellationToken cancellationToken = default)
    {
        var items = await context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => !warehouseId.HasValue || i.WarehouseId == warehouseId.Value)
            .ToListAsync(cancellationToken);

        // Filter by calculated property client-side
        return items.Where(i => i.QuantityAvailable <= i.LowStockThreshold);
    }
}
