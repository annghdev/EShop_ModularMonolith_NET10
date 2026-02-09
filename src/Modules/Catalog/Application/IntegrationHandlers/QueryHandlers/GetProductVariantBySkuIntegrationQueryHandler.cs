using Contracts;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application;

public class GetProductVariantBySkuIntegrationQueryHandler(ICatalogUnitOfWork uow)
    : IRequestHandler<GetProductVariantBySkuIntegrationQuery, ProductVariantBySkuResponse>
{
    public async Task<ProductVariantBySkuResponse> Handle(
        GetProductVariantBySkuIntegrationQuery query,
        CancellationToken cancellationToken)
    {
        var product = await uow.Products.AsQueryable()
            .Include(p => p.Variants)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Variants.Any(v => v.Sku.Value == query.Sku), cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product", query.Sku);
        }

        var variant = product.Variants.First(v => v.Sku.Value == query.Sku);

        return new ProductVariantBySkuResponse(
            product.Id,
            product.Name,
            variant.Id,
            variant.Sku.Value,
            product.Thumbnail?.Path);
    }
}
