using Contracts;

namespace Inventory.Application;

public class GetProductQuantityIntegrationQueryHandler
    : IRequestHandler<GetProductQuantityIntegrationQuery, ProductQuantityResponse>
{
    public Task<ProductQuantityResponse> Handle(GetProductQuantityIntegrationQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ProductQuantityResponse
        {
            ProductId = request.ProductId,
            Variants = new List<VariantQuantityResponse>() { new VariantQuantityResponse(Guid.NewGuid(), 100) }
        });
    }
}
