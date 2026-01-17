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

public record InventoryReceivedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    string Sku,
    int ReceivedQuantity,
    int TotalQuantity) : DomainEvent;

public record InventoryShippedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    string Sku,
    int ShippedQuantity,
    int RemainingQuantity) : DomainEvent;

public record InventoryReservedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    Guid OrderId,
    int ReservedQuantity,
    int AvailableQuantity) : DomainEvent;

public record InventoryReleasedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    Guid OrderId,
    int ReleasedQuantity,
    int AvailableQuantity) : DomainEvent;

public record InventoryConfirmedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    Guid OrderId,
    int ConfirmedQuantity,
    int RemainingQuantity) : DomainEvent;

public record InventoryAdjustedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    int AdjustmentQuantity,
    int NewQuantity,
    string Reason) : DomainEvent;

public record LowStockWarningEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    string Sku,
    int AvailableQuantity,
    int Threshold) : DomainEvent;

public record InventoryMovementCreatedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    Guid? OrderId,
    int Quantity,
    MovementType Type,
    int SnapshotQuantity,
    string? Reference) : DomainEvent;

public record InventoryReservationFailedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    Guid OrderId,
    int RequestedQuantity,
    int AvailableQuantity,
    string Reason) : DomainEvent;
