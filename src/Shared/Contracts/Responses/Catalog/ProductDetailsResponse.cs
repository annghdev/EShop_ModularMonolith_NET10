namespace Contracts.Responses;

public class ProductDetailsResponse
{
    public ProductDto Product { get; set; } = new();
    public List<VariantQuantityDto> VariantQuantities { get; set; } = [];
}

public record VariantQuantityDto(Guid VariantId, int QuantityAvailable);
