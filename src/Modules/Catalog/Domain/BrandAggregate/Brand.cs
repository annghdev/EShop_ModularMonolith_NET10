namespace Catalog.Domain;

public class Brand : AggregateRoot
{
    public required string Name { get; set; }
    public string? Logo { get; set; }
}
