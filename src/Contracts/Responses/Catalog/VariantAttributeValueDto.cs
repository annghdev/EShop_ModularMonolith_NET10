namespace Contracts.Responses;

public class VariantAttributeValueDto
{
    public Guid ProductAttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public Guid ValueId { get; set; }
    public string ValueName { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
}
