namespace Contracts.Responses;

public class ProductAttributeDto
{
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool HasVariant { get; set; }
}
