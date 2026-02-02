namespace Contracts.Responses;

public class CategoryDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public Guid? ParentId { get; set; }
    public int Level { get; set; }
    public List<CategoryDefaultAttributeDto> DefaultAttributes { get; set; } = [];
    public List<CategoryDefaultAttributeDto> OwnDefaultAttributes { get; set; } = [];
}
