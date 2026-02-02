namespace Inventory.Domain;

public record WarehouseCreatedEvent(
    Guid WarehouseId,
    string Code,
    string Name,
    bool IsDefault) : DomainEvent;

public record WarehouseActivatedEvent(
    Guid WarehouseId,
    string Code) : DomainEvent;

public record WarehouseDeactivatedEvent(
    Guid WarehouseId,
    string Code) : DomainEvent;