using Users.Domain;

namespace Users.Infrastructure.EFCore.Repositories;

public class EmployeeRepository(UsersDbContext db)
    : BaseRepository<Employee, UsersDbContext>(db), IEmployeeRepository
{
    public override async Task<Employee> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException("Employee", id);
    }

    public override async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken ct = default)
    {
        var normalizedCode = employeeCode.ToUpperInvariant();

        return await dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeCode == normalizedCode, ct);
    }

    public async Task<Employee?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.AccountId == accountId, ct);
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        return await dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Email.Value == normalizedEmail, ct);
    }

    public async Task<bool> ExistsAsync(string employeeCode, CancellationToken ct = default)
    {
        var normalizedCode = employeeCode.ToUpperInvariant();

        return await dbSet
            .AsNoTracking()
            .AnyAsync(e => e.EmployeeCode == normalizedCode, ct);
    }
}
