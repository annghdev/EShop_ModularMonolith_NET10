namespace Catalog.Domain;

public class AttributeValue : BaseEntity
{
    public required string Name { get; set; }
    public string? ColorCode { get; set; }
    public Guid AttributeId { get; set; }
    public Attribute? Attribute { get; set; }
}
