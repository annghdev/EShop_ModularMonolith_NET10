namespace Catalog.Domain;

public class Collection : AggregateRoot
{
    public required string Name { get; set; }
    public string? Description  { get; set; }

    public List<CollectionItem> Items { get; set; } = [];
}
