namespace Catalog.Application;

public class VariantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public MoneyDto? OverrideCost { get; set; }
    public MoneyDto? OverridePrice { get; set; }
    public DimensionsDto? Dimensions { get; set; }
    public string? MainImage { get; set; }
    public List<string> Images { get; set; } = new();
    public List<VariantAttributeValueDto> AttributeValues { get; set; } = new();
}

public class VariantAttributeValueDto
{
    public Guid ProductAttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public Guid ValueId { get; set; }
    public string ValueName { get; set; } = string.Empty;
}

public record VariantRecordDto(
    Guid Id,
    string? Name,
    string Sku,
    MoneyDto OverrideCost,
    MoneyDto OverridePrice,
    DimensionsDto Dimensions
    );

