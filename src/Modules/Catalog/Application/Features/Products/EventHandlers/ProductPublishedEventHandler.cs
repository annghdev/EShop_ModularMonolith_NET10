using Catalog.Domain;
using Contracts.IntegrationEvents.CatalogEvents;
using Elastic.Clients.Elasticsearch;

namespace Catalog.Application;

public class ProductPublishedEventHandler(ElasticsearchClient elasticsearchClient, IIntegrationEventPublisher publisher)
    : INotificationHandler<ProductPublishedEvent>
{
    private const string IndexName = "products";

    public async Task Handle(ProductPublishedEvent notification, CancellationToken cancellationToken)
    {
        var t1 = SyncElasticSearch(notification.Payload, cancellationToken);
        var t2 = PublishIntegrationEvent(notification.Payload, cancellationToken);

        await Task.WhenAll(t1, t2);
    }

    private async Task SyncElasticSearch(Product product, CancellationToken cancellationToken)
    {
        var productProjection = ProductMappingProfile.MapToProjection(product);

        var response = await elasticsearchClient.IndexAsync(
            productProjection,
            i => i
                .Index(IndexName)
                .Id(productProjection.Id),
            cancellationToken);

        if (!response.IsSuccess())
        {
            // Log error but don't throw - event handling should be resilient
            Console.WriteLine($"Failed to index product {productProjection.Id}: {response.DebugInformation}");
        }
    }

    private async Task PublishIntegrationEvent(Product product, CancellationToken cancellationToken)
    {
        var integrationEvent = new ProductPublishedIntegrationEvent(
                product.Id,
                product.Name,
                product.SkuPrefix,
                product.Thumbnail?.Path,
                product.Variants
                    .Select(v => new ProductVariantPublishDto(v.Id, v.Name, v.Sku.Value))
                    .ToList()
            );

        await publisher.PublishAsync(integrationEvent, cancellationToken);
    }

}
