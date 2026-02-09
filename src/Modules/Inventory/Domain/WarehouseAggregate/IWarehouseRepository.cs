namespace Inventory.Domain;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
}
