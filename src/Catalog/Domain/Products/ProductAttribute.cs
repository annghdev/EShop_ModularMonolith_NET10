namespace Catalog.Domain;

public class ProductAttribute : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid AttributeId { get; set; }
    public Attribute? Attribute { get; set; }

    public Guid? DefaultValueId { get; set; }
    public AttributeValue? DefaultValue { get; set; }

    public int DisplayOrder { get; set; }
}
