namespace Contracts.Requests.Catalog;

public class CreateAttributeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool DisplayText { get; set; }
    public string? ValueStyleCss { get; set; }
}
