namespace Contracts;

public record ProductQuantityResponse
{
    public Guid ProductId { get; init; }
    public List<VariantQuantityResponse> Variants { get; init; } = new();
}
public record VariantQuantityResponse(Guid VariantId, int Quantity);
