namespace Contracts.Requests.Catalog;

public class CreateBrandRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
}
