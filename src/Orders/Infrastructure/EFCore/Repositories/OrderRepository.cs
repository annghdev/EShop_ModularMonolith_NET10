using Orders.Domain;

namespace Orders.Infrastructure.EFCore.Repositories;

public class OrderRepository(OrdersDbContext db)
    : BaseRepository<Order, OrdersDbContext>(db), IOrderRepository
{
    public override async Task<Order> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException("Order", id);
    }

    public override async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .Include(o => o.StatusHistory)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }
}
