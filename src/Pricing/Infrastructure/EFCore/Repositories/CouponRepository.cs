using Pricing.Domain;

namespace Pricing.Infrastructure.EFCore.Repositories;

public class CouponRepository(PricingDbContext db)
    : BaseRepository<Coupon, PricingDbContext>(db), ICouponRepository
{
    public override async Task<Coupon> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(c => c.Conditions)
            .Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Coupon", id);
    }

    public override async Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Conditions)
            .Include(c => c.Usages)
            .ToListAsync(cancellationToken);
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.ToUpperInvariant();

        return await dbSet
            .AsNoTracking()
            .Include(c => c.Conditions)
            .Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Code == normalizedCode, cancellationToken);
    }

    public async Task<IEnumerable<Coupon>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbSet
            .AsNoTracking()
            .Where(c => c.Status == CouponStatus.Active && c.StartDate <= now && c.EndDate >= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Coupon>> GetExpiredActiveAsync(
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Where(c => c.Status == CouponStatus.Active && c.EndDate < currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.ToUpperInvariant();

        return await dbSet
            .AsNoTracking()
            .AnyAsync(c => c.Code == normalizedCode, cancellationToken);
    }
}
