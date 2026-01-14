namespace ShoppingCart.Domain;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByGuestIdAsync(string guestId, CancellationToken cancellationToken = default);
    Task<Cart?> GetActiveCartAsync(Guid? customerId, string? guestId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cart>> GetAbandonedCartsAsync(TimeSpan inactivityThreshold, CancellationToken cancellationToken = default);
}
