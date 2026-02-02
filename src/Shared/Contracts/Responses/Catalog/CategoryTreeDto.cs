namespace Contracts.Responses;

public class CategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public List<CategoryTreeDto> Children { get; set; } = [];
}
