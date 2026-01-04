using Catalog.Domain;
using Elastic.Clients.Elasticsearch;

namespace Catalog.Application;

public class ProductPublishedEventHandler(ElasticsearchClient elasticsearchClient)
    : INotificationHandler<ProductPublishedEvent>
{
    private const string IndexName = "products";

    public async Task Handle(ProductPublishedEvent notification, CancellationToken cancellationToken)
    {
        var productProjection = MapToProjection(notification.Payload);

        var response = await elasticsearchClient.IndexAsync(
            productProjection,
            i => i
                .Index(IndexName)
                .Id(productProjection.Id),
            cancellationToken
        );

        if (!response.IsSuccess())
        {
            // Log error but don't throw - event handling should be resilient
            Console.WriteLine($"Failed to index product {productProjection.Id}: {response.DebugInformation}");
        }
    }

    private static ProductProjection MapToProjection(Product product)
    {
        return new ProductProjection
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Slug = product.Slug?.Value ?? string.Empty,
            Sku = product.Sku.Value,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandName = product.Brand?.Name ?? string.Empty,
            Status = product.Status.ToString(),
            Attributes = product.Attributes.Select(pa => new ProductAttributeProjection
            {
                AttributeName = pa.Attribute?.Name ?? string.Empty,
                ValueName = pa.DefaultValue?.Name ?? string.Empty
            }).ToList(),
            Variants = product.Variants.Select(v => new ProductVariantProjection
            {
                Id = v.Id.ToString(),
                Name = v.Name,
                Sku = v.Sku.Value,
                Price = v.OverridePrice?.Amount ?? product.Price.Amount,
                Currency = v.OverridePrice?.Currency ?? product.Price.Currency
            }).ToList(),
            CreatedAt = product.CreatedAt.DateTime,
            UpdatedAt = product.UpdatedAt?.DateTime
        };
    }
}
