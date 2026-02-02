namespace Catalog.Domain;

public class CategoryDefaultAttribute : BaseEntity
{
    public Guid AttributeId { get; set; }
    public Attribute? Attribute { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public int DisplayOrder { get; set; }
}