namespace Contracts;

public record GetProductQuantityIntegrationQuery(
    string CallFrom,
    Guid ProductId) : IIntegrationQuery<ProductQuantityResponse>;
