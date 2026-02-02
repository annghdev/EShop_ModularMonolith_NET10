namespace Contracts.IntegrationEvents.InventoryEvents;

public record InventoryReserveFailedIntegrationEvent(Guid OrderId) : IntegrationEvent;