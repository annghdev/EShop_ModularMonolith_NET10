namespace Catalog.Application;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string Sku { get; set; } = string.Empty;
    public MoneyDto? Cost { get; set; }
    public MoneyDto? Price { get; set; }
    public DimensionsDto? Dimensions { get; set; }
    public bool? HastockQuantity { get; set; }
    public string? Thumbnail { get; set; }
    public List<string> Images { get; set; } = [];
    public CategoryDto Category { get; set; } = new();
    public BrandDto Brand { get; set; } = new();
    public int DisplayPriority { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ProductAttributeDto> Attributes { get; set; } = [];
    public List<VariantDto> Variants { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ProductAttributeDto
{
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool HasVariant { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


public class ProductSearchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public MoneyDto? Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
