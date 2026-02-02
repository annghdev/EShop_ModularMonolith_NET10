using Payment.Domain;

namespace Payment.Infrastructure.EFCore.Repositories;

public class PaymentGatewayRepository(PaymentDbContext db)
    : BaseRepository<PaymentGateway, PaymentDbContext>(db), IPaymentGatewayRepository
{
    public override async Task<PaymentGateway> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(p => p.Setting)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("PaymentGateway", id);
    }

    public override async Task<IEnumerable<PaymentGateway>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.Setting)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentGateway?> GetByProviderAsync(PaymentProvider provider, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.Setting)
            .FirstOrDefaultAsync(p => p.Provider == provider, cancellationToken);
    }

    public async Task<List<PaymentGateway>> GetActiveGatewaysAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.Setting)
            .Where(p => p.IsEnabled && p.Status == PaymentGatewayStatus.Active)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentGateway?> GetByIdWithConfigurationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .Include(p => p.Setting)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
