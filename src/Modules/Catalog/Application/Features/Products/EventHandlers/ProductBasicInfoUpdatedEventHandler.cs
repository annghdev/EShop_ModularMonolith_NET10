using Catalog.Domain;
using Contracts.IntegrationEvents;
using Elastic.Clients.Elasticsearch;

namespace Catalog.Application.Features.Products.EventHandlers;

public class ProductBasicInfoUpdatedEventHandler(ElasticsearchClient elasticsearchClient, IIntegrationEventPublisher publisher)
    : INotificationHandler<ProductBasicInfoUpdatedEvent>
{
    private const string IndexName = "products";

    public async Task Handle(ProductBasicInfoUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var t1 = SyncElasticSearch(notification.Payload, cancellationToken);
        var t2 = PublishIntegrationEvent(notification.Payload.Id, cancellationToken);

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

    private async Task PublishIntegrationEvent(Guid productId, CancellationToken cancellationToken)
    {
        var integrationEvent = new ProductBasicInfoUpdatedIntegrationEvent(productId);

        await publisher.PublishAsync(integrationEvent, cancellationToken);
    }
}
