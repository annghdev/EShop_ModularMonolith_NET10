namespace Contracts.Requests.Catalog;

public class UpdateBrandRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
}
