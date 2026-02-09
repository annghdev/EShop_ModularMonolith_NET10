namespace Contracts.Requests.Inventory;

/// <summary>
/// Request to create a new warehouse
/// </summary>
public record CreateWarehouseRequest(
    string Code,
    string Name,
    string? Street = null,
    string? City = null,
    string? State = null,
    string? Country = null,
    string? ZipCode = null,
    bool IsDefault = false);

/// <summary>
/// Request to update a warehouse
/// </summary>
public record UpdateWarehouseRequest(
    string Name,
    string? Street = null,
    string? City = null,
    string? State = null,
    string? Country = null,
    string? ZipCode = null);
