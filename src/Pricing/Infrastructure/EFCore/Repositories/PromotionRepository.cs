using Pricing.Domain;

namespace Pricing.Infrastructure.EFCore.Repositories;

public class PromotionRepository(PricingDbContext db)
    : BaseRepository<Promotion, PricingDbContext>(db), IPromotionRepository
{
    public override async Task<Promotion> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(p => p.Rules)
            .Include(p => p.Actions)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Promotion", id);
    }

    public override async Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.Rules)
            .Include(p => p.Actions)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetActiveAsync(
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Where(p => p.Status == PromotionStatus.Active && p.StartDate <= currentTime && p.EndDate >= currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetByTypeAsync(
        PromotionType type,
        CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Where(p => p.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetActiveByTypeAsync(
        PromotionType type,
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Where(p => p.Type == type && p.Status == PromotionStatus.Active && p.StartDate <= currentTime && p.EndDate >= currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetExpiredActiveAsync(
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Where(p => p.Status == PromotionStatus.Active && p.EndDate < currentTime)
            .ToListAsync(cancellationToken);
    }
}
