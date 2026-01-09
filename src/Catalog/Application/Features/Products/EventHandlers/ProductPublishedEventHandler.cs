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
        var productProjection = MapToProjection(product);

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
                product.Variants
                    .Select(v => new ProductVariantPublishDto(v.Id, v.Name, v.Sku.Value))
                    .ToList()
            );

        await publisher.PublishAsync(integrationEvent, cancellationToken);
    }

    private static ProductProjection MapToProjection(Product product)
    {
        return new ProductProjection
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Slug = product.Slug?.Value ?? string.Empty,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandName = product.Brand?.Name ?? string.Empty,
            Status = product.Status.ToString(),
            Attributes = product.Attributes.Select(pa => new ProductAttributeProjection
            {
                AttributeId = pa.AttributeId.ToString(),
                AttributeName = pa.Attribute?.Name ?? string.Empty
            }).ToList(),
            Variants = product.Variants.Select(v => new ProductVariantProjection
            {
                Id = v.Id.ToString(),
                Name = v.Name,
                Sku = v.Sku.Value,
                Price = v.OverridePrice?.Amount ?? product.Price.Amount,
                Currency = v.OverridePrice?.Currency ?? product.Price.Currency,
                Attributes = v.AttributeValues.Select(av => new VariantAttributeProjection
                {
                    AttributeId = av.ProductAttributeId.ToString(),
                    AttributeName = av.ProductAttribute?.Attribute?.Name ?? string.Empty,
                    ValueId = av.ValueId.ToString(),
                    ValueName = av.Value?.Name ?? string.Empty
                }).ToList()
            }).ToList(),
            CreatedAt = product.CreatedAt.DateTime,
            UpdatedAt = product.UpdatedAt?.DateTime
        };
    }
}
