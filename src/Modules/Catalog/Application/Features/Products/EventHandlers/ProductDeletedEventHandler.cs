using Catalog.Domain;
using Contracts.IntegrationEvents.CatalogEvents;
using Elastic.Clients.Elasticsearch;

namespace Catalog.Application;

public class ProductDeletedEventHandler(IIntegrationEventPublisher publisher, ElasticsearchClient elasticsearchClient) : INotificationHandler<ProductDeletedEvent>
{
    public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        var t1 = SyncElasticSearch(notification.ProductId);
        var t2 = PublishIntegrationEvent(notification.ProductId, cancellationToken);

        await Task.WhenAll(t1, t2);
    }

    private async Task SyncElasticSearch(Guid productId)
    {
        try
        {
            var response = await elasticsearchClient.DeleteAsync<ProductProjection>(
                productId.ToString(),
                d => d.Index("products")
            );

            if (!response.IsSuccess())
            {
                // Log warning but don't throw - product deletion should succeed even if ES sync fails
                Console.WriteLine($"Failed to delete product {productId} from Elasticsearch: {response.DebugInformation}");
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - product deletion should succeed even if ES sync fails
            Console.WriteLine($"Error deleting product {productId} from Elasticsearch: {ex.Message}");
        }
    }

    private async Task PublishIntegrationEvent(Guid productId, CancellationToken cancellationToken)
    {
        await publisher.PublishAsync(new ProductDeletedIntegrationEvent(productId), cancellationToken);
    }
}
