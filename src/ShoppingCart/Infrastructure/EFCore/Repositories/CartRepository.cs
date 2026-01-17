using ShoppingCart.Domain;

namespace ShoppingCart.Infrastructure.EFCore.Repositories;

public class CartRepository(ShoppingCartDbContext db)
    : BaseRepository<Cart, ShoppingCartDbContext>(db), ICartRepository
{
    public override async Task<Cart> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Cart", id);
    }

    public override async Task<IEnumerable<Cart>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Customer.CustomerId == customerId, cancellationToken);
    }

    public async Task<Cart?> GetByGuestIdAsync(string guestId, CancellationToken cancellationToken = default)
    {
        var normalizedGuestId = guestId.Trim();

        return await dbSet
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Customer.GuestId == normalizedGuestId, cancellationToken);
    }

    public async Task<Cart?> GetActiveCartAsync(
        Guid? customerId,
        string? guestId,
        CancellationToken cancellationToken = default)
    {
        var query = dbSet
            .AsNoTracking()
            .Include(c => c.Items)
            .Where(c => c.Status == CartStatus.Active);

        if (customerId.HasValue)
        {
            query = query.Where(c => c.Customer.CustomerId == customerId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(guestId))
        {
            var normalizedGuestId = guestId.Trim();
            query = query.Where(c => c.Customer.GuestId == normalizedGuestId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Cart>> GetAbandonedCartsAsync(
        TimeSpan inactivityThreshold,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(inactivityThreshold);

        return await dbSet
            .AsNoTracking()
            .Include(c => c.Items)
            .Where(c => c.Status == CartStatus.Active && c.LastActivityAt <= cutoff)
            .ToListAsync(cancellationToken);
    }
}
