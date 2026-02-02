namespace Contracts.Requests.Catalog;

public class UpdateAttributeValueRequest
{
    public string Value { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
}
