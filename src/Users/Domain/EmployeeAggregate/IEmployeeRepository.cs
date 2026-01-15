namespace Users.Domain;

/// <summary>
/// Repository interface for Employee aggregate
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken ct = default);
    Task<Employee?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsAsync(string employeeCode, CancellationToken ct = default);
}
