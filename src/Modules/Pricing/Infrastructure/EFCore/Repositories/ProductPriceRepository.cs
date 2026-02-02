using Pricing.Domain;

namespace Pricing.Infrastructure.EFCore.Repositories;

public class ProductPriceRepository(PricingDbContext db)
    : BaseRepository<ProductPrice, PricingDbContext>(db), IProductPriceRepository
{
    public override async Task<ProductPrice> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(p => p.ChangeLogs)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("ProductPrice", id);
    }

    public override async Task<IEnumerable<ProductPrice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.ChangeLogs)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductPrice?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.ChangeLogs)
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
    }

    public async Task<ProductPrice?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.ChangeLogs)
            .FirstOrDefaultAsync(p => p.VariantId == variantId, cancellationToken);
    }

    public async Task<ProductPrice?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalizedSku = sku.ToUpperInvariant();

        return await dbSet
            .AsNoTracking()
            .Include(p => p.ChangeLogs)
            .FirstOrDefaultAsync(p => p.Sku.Value == normalizedSku, cancellationToken);
    }

    public async Task<IEnumerable<ProductPrice>> GetByProductIdsAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.ChangeLogs)
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync(cancellationToken);
    }
}
