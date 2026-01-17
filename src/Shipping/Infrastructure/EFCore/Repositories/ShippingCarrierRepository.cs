using Shipping.Domain;

namespace Shipping.Infrastructure.EFCore.Repositories;

public class ShippingCarrierRepository(ShippingDbContext db)
    : BaseRepository<ShippingCarrier, ShippingDbContext>(db), IShippingCarrierRepository
{
    public override async Task<ShippingCarrier> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(c => c.Setting)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("ShippingCarrier", id);
    }

    public override async Task<IEnumerable<ShippingCarrier>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Setting)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<ShippingCarrier?> GetByProviderAsync(ShippingProvider provider, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Setting)
            .FirstOrDefaultAsync(c => c.Provider == provider, ct);
    }

    public async Task<ShippingCarrier?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Setting)
            .FirstOrDefaultAsync(c => c.Name == name, ct);
    }

    public async Task<List<ShippingCarrier>> GetEnabledCarriersAsync(CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Setting)
            .Where(c => c.IsEnabled && c.Status == ShippingCarrierStatus.Active)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<List<ShippingCarrier>> GetByRegionAsync(string region, CancellationToken ct = default)
    {
        var carriers = await dbSet
            .AsNoTracking()
            .Include(c => c.Setting)
            .Where(c => c.IsEnabled && c.Status == ShippingCarrierStatus.Active)
            .ToListAsync(ct);

        return carriers
            .Where(c => c.SupportsRegion(region))
            .ToList();
    }
}
