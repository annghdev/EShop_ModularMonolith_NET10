namespace Contracts.IntegrationEvents.InventoryEvents;

public record InventoryReserveSucceededIntegrationEvent(Guid OrderId) : IntegrationEvent;