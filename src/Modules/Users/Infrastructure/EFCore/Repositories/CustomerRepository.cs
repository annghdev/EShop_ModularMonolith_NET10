using Users.Domain;

namespace Users.Infrastructure.EFCore.Repositories;

public class CustomerRepository(UsersDbContext db)
    : BaseRepository<Customer, UsersDbContext>(db), ICustomerRepository
{
    public override async Task<Customer> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Customer", id);
    }

    public override async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Addresses)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        return await dbSet
            .AsNoTracking()
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Email.Value == normalizedEmail, ct);
    }

    public async Task<Customer?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.AccountId == accountId, ct);
    }

    public async Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        return await dbSet
            .AsNoTracking()
            .AnyAsync(c => c.Email.Value == normalizedEmail, ct);
    }
}
