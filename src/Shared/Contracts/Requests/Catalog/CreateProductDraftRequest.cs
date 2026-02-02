using Contracts;

namespace Contracts.Requests.Catalog;

public class CreateProductDraftRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public MoneyDto Cost { get; set; } = new(0);
    public MoneyDto Price { get; set; } = new(0);
    public DimensionsDto Dimensions { get; set; } = new(0, 0, 0, 0);
    public bool HasStockQuantity { get; set; } = true;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public int DisplayPriority { get; set; } = 1;
    public string? Thumbnail { get; set; }
    public List<string> Images { get; set; } = [];
    public List<AddProductAttributeDto> Attributes { get; set; } = [];
    public List<AddVariantDto> Variants { get; set; } = [];
}

public class AddVariantDto
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public MoneyDto? OverrideCost { get; set; }
    public MoneyDto? OverridePrice { get; set; }
    public DimensionsDto? Dimensions { get; set; }
    public string? MainImage { get; set; }
    public List<string> Images { get; set; } = [];
    public Dictionary<Guid, Guid> AttributeValues { get; set; } = new();
}

public class AddProductAttributeDto
{
    public Guid AttributeId { get; set; }
    public Guid Value { get; set; }
    public int DisplayOrder { get; set; }
}
