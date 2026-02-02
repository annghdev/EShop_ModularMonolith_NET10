using Inventory.Domain;

namespace Inventory.Infrastructure;

public class WarehouseRepository(InventoryDbContext context) : IWarehouseRepository
{
    public void Add(Warehouse entity)
    {
        context.Warehouses.Add(entity);
    }

    public void Update(Warehouse entity)
    {
        context.Warehouses.Update(entity);
    }

    public void Remove(Warehouse entity)
    {
        context.Warehouses.Remove(entity);
    }

    public async Task<Warehouse> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = context.Warehouses
            .Include(w => w.InventoryItems)
                .ThenInclude(i => i.Reservations)
            .AsQueryable();

        if (!changeTracking)
            query = query.AsNoTracking();

        return await query.FirstAsync(w => w.Id == id);
    }

    public async Task<bool> CheckExistsAsync(Guid id)
    {
        return await context.Warehouses.AnyAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Warehouses.ToListAsync(cancellationToken);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await context.Warehouses
            .Include(w => w.InventoryItems)
                .ThenInclude(i => i.Reservations)
            .FirstOrDefaultAsync(w => w.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken cancellationToken = default)
    {
        return await context.Warehouses
            .Include(w => w.InventoryItems)
                .ThenInclude(i => i.Reservations)
            .FirstOrDefaultAsync(w => w.IsDefault && w.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Warehouses
            .Where(w => w.IsActive)
            .ToListAsync(cancellationToken);
    }
}
