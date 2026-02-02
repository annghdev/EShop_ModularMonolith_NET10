namespace Contracts.Requests.Catalog;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public Guid? ParentId { get; set; }
    public List<CategoryDefaultAttributeRequest> DefaultAttributes { get; set; } = [];
}
