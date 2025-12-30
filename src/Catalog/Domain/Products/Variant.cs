namespace Catalog.Domain;

public class Variant : AuditableEntity
{
    public required string Sku { get; set; }
    public Money? OverrideCost { get; set; }
    public Money? OverridePrice { get; set; }
    public ImageUrl? MainImage { get; set; }
    public List<ImageUrl> Images { get; set; } = [];
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public List<VariantAttributeValue> AttributeValues { get; set; } = [];
}
