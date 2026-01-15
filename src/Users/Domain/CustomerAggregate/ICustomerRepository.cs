namespace Users.Domain;

/// <summary>
/// Repository interface for Customer aggregate
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Customer?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(string email, CancellationToken ct = default);
}
