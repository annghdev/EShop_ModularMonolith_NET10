namespace Pricing.Domain;

public interface IProductPriceRepository : IRepository<ProductPrice>
{
    Task<ProductPrice?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductPrice?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task<ProductPrice?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductPrice>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
}
