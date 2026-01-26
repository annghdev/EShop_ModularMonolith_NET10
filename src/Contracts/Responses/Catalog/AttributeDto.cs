namespace Contracts.Responses;

public class AttributeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public List<AttributeValueDto> Values { get; set; } = [];
}

public class AttributeValueDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
}
