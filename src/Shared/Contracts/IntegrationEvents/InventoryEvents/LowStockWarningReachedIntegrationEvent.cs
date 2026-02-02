namespace Contracts.IntegrationEvents.InventoryEvents;

public record LowStockWarningReachedIntegrationEvent(
    Guid ItemId,
    string Name,
    string Sku,
    int Quantity) : IntegrationEvent;