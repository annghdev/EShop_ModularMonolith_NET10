namespace Contracts;

public record GetProductVariantBySkuIntegrationQuery(
    string CallFrom,
    string Sku) : IIntegrationQuery<ProductVariantBySkuResponse>;

public record ProductVariantBySkuResponse(
    Guid ProductId,
    string ProductName,
    Guid VariantId,
    string VariantName,
    string Sku,
    string? Thumbnail,
    MoneyDto UnitPrice);
