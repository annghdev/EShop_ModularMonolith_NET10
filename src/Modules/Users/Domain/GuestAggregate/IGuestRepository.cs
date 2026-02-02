namespace Users.Domain;

/// <summary>
/// Repository interface for Guest aggregate
/// </summary>
public interface IGuestRepository : IRepository<Guest>
{
    Task<Guest?> GetByGuestIdAsync(string guestId, CancellationToken ct = default);
    Task<Guest?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Guest>> GetUnconvertedGuestsAsync(DateTimeOffset olderThan, CancellationToken ct = default);
}
