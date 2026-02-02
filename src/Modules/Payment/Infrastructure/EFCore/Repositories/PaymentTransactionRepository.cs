using Payment.Domain;

namespace Payment.Infrastructure.EFCore.Repositories;

public class PaymentTransactionRepository(PaymentDbContext db)
    : BaseRepository<PaymentTransaction, PaymentDbContext>(db), IPaymentTransactionRepository
{
    public override async Task<PaymentTransaction> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(p => p.Gateway)
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("PaymentTransaction", id);
    }

    public override async Task<IEnumerable<PaymentTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.Gateway)
            .Include(p => p.Refunds)
            .OrderByDescending(p => p.InitiatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.Gateway)
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<PaymentTransaction?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.Gateway)
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.TransactionNumber == transactionNumber, cancellationToken);
    }

    public async Task<PaymentTransaction?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(p => p.Gateway)
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Metadata.ExternalTransactionId == externalId, cancellationToken);
    }

    public async Task<List<PaymentTransaction>> GetExpiredTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbSet
            .Where(p => (p.Status == PaymentTransactionStatus.Pending || p.Status == PaymentTransactionStatus.Processing)
                        && p.ExpiresAt.HasValue
                        && p.ExpiresAt.Value < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentTransaction?> GetByIdWithRefundsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
