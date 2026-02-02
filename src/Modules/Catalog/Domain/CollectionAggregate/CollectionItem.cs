namespace Catalog.Domain;

public class CollectionItem : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid CollectionId { get; set; }
    public Collection? Collection { get; set; }
}
