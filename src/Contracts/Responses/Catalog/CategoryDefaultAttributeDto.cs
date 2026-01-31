namespace Contracts.Responses;

public class CategoryDefaultAttributeDto
{
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
