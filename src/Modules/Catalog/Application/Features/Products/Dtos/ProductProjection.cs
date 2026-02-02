namespace Catalog.Application;

public sealed class ProductProjection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandId { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ProductAttributeProjection> Attributes { get; set; } = new();
    public List<ProductVariantProjection> Variants { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class ProductAttributeProjection
{
    public string AttributeId { get; set; } = string.Empty;
    public string AttributeName { get; set; } = string.Empty;
}

public sealed class ProductVariantProjection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";

    public List<VariantAttributeProjection> Attributes { get; set; } = new();
}

public sealed class VariantAttributeProjection
{
    public string AttributeId { get; set; } = string.Empty;
    public string AttributeName { get; set; } = string.Empty;
    public string ValueId { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
}
