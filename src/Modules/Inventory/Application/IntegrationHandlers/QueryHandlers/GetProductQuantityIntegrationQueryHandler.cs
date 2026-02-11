using Contracts;

namespace Inventory.Application;

public class GetProductQuantityIntegrationQueryHandler(IInventoryUnitOfWork uow)
    : IRequestHandler<GetProductQuantityIntegrationQuery, ProductQuantityResponse>
{
    public async Task<ProductQuantityResponse> Handle(GetProductQuantityIntegrationQuery request, CancellationToken cancellationToken)
    {
        var items = await uow.InventoryItems
            .AsNoTracking()
            .Where(i => i.ProductId == request.ProductId)
            .ToListAsync(cancellationToken);

        var variants = items
            .GroupBy(i => i.VariantId)
            .Select(g => new VariantQuantityResponse(
                g.Key,
                g.Sum(i => i.QuantityAvailable)))
            .ToList();

        return new ProductQuantityResponse
        {
            ProductId = request.ProductId,
            Variants = variants
        };
    }
}
