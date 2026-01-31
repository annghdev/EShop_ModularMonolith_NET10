namespace Contracts.Requests.Catalog;

public class AddAttributeValueRequest
{
    public string Value { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
}
