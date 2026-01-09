namespace Catalog.Domain;

public class Category : AggregateRoot
{
    public required string Name { get; set; }
    public string? Image { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<CategoryDefaultAttribute> DefaultAttributes { get; set; } = [];

    public IEnumerable<CategoryDefaultAttribute> GetAllDefaultAttributesFromHierarchy()
    {
        var allAttributes = new List<CategoryDefaultAttribute>();
        var current = this;

        // Traverse up the hierarchy to collect all default attributes
        while (current != null)
        {
            allAttributes.AddRange(current.DefaultAttributes);
            current = current.Parent;
        }

        // Return distinct attributes (in case of duplicates in hierarchy)
        return allAttributes.DistinctBy(attr => attr.AttributeId);
    }
}
