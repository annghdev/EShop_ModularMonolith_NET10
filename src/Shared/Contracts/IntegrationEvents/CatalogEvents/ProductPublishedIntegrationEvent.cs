namespace Contracts.IntegrationEvents.CatalogEvents;

public record ProductPublishedIntegrationEvent(Guid ProductId, IEnumerable<ProductVariantPublishDto> Payload) : IntegrationEvent;

public record ProductVariantPublishDto(
    Guid Id,
    string Name,
    string Sku);