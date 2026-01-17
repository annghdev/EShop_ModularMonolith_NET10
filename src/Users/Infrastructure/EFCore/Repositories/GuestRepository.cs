using Users.Domain;

namespace Users.Infrastructure.EFCore.Repositories;

public class GuestRepository(UsersDbContext db)
    : BaseRepository<Guest, UsersDbContext>(db), IGuestRepository
{
    public override async Task<Guest> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new NotFoundException("Guest", id);
    }

    public override async Task<IEnumerable<Guest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Guest?> GetByGuestIdAsync(string guestId, CancellationToken ct = default)
    {
        var normalizedId = guestId.Trim();

        return await dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.GuestId == normalizedId, ct);
    }

    public async Task<Guest?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        return await dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Email == normalizedEmail, ct);
    }

    public async Task<IReadOnlyList<Guest>> GetUnconvertedGuestsAsync(DateTimeOffset olderThan, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Where(g => !g.IsConverted && g.LastActivityAt <= olderThan)
            .OrderBy(g => g.LastActivityAt)
            .ToListAsync(ct);
    }
}
