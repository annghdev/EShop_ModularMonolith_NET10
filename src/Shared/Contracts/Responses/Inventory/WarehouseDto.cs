namespace Contracts.Responses.Inventory;

/// <summary>
/// Represents a warehouse
/// </summary>
public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string? Street,
    string? City,
    string? State,
    string? Country,
    string? ZipCode,
    bool IsActive,
    bool IsDefault);

/// <summary>
/// Simple warehouse reference for dropdowns
/// </summary>
public record WarehouseSimpleDto(
    Guid Id,
    string Code,
    string Name,
    bool IsDefault);
