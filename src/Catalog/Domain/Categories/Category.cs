namespace Catalog.Domain;

public class Category : AggregateRoot
{
    public required string Name { get; set; }
    public string? Image { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<string> DefaultAttributes { get; set; } = [];
}
