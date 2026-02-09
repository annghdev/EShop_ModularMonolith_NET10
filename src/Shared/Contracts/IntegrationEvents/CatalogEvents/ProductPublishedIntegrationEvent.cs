namespace Contracts.IntegrationEvents.CatalogEvents;

public record ProductPublishedIntegrationEvent(
    Guid ProductId,
    string ProductName,
    string? SkuPrefix,
    string? Thumbnail,
    IEnumerable<ProductVariantPublishDto> Variants) : IntegrationEvent;

public record ProductVariantPublishDto(
    Guid Id,
    string Name,
    string Sku);