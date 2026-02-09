using Contracts.Requests.Inventory;
using Contracts.Responses.Inventory;

namespace BlazorAdmin.Services;

/// <summary>
/// Service interface for warehouse operations
/// </summary>
public interface IWarehouseService
{
    // Queries
    Task<List<WarehouseDto>> GetAllAsync();
    
    Task<List<WarehouseSimpleDto>> GetActiveAsync();
    
    Task<WarehouseDto?> GetByIdAsync(Guid id);
    
    // Commands
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request);
    
    Task UpdateAsync(Guid id, UpdateWarehouseRequest request);
    
    Task DeleteAsync(Guid id);
}
