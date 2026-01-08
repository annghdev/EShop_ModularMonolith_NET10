namespace Catalog.Domain;

public class ProductAttribute : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }

    public Guid AttributeId { get; private set; }
    public Attribute? Attribute { get; private set; }

    public int DisplayOrder { get; private set; }
    public bool HasVariant { get; set; }

    private ProductAttribute() { } // EF Core

    public ProductAttribute(
        Guid attributeId,
        int displayOrder)
    {
        AttributeId = attributeId;
        DisplayOrder = displayOrder;
    }

    public void UpdateDisplayOrder(int order)
    {
        if (order < 0)
            throw new DomainException("Display order cannot be negative");

        DisplayOrder = order;
    }
}
