using Contracts.Responses;

namespace Contracts.Responses.Catalog;

public class VariantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public MoneyDto? OverrideCost { get; set; }
    public MoneyDto? OverridePrice { get; set; }
    public DimensionsDto? Dimensions { get; set; }
    public string? MainImage { get; set; }
    public List<string> Images { get; set; } = [];
    public List<VariantAttributeValueDto> AttributeValues { get; set; } = [];
}
