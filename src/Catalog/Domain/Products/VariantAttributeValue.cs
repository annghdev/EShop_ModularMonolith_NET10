namespace Catalog.Domain;

public class VariantAttributeValue : BaseEntity
{
    public Guid VariantId { get; set; }
    public Variant? Variant { get; set; }

    public Guid ValueId { get; set; }
    public AttributeValue? Value { get; set; }

    public Guid ProductAttributeId { get; set; }
    public ProductAttribute? ProductAttribute { get; set; }
}
