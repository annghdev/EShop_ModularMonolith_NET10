namespace Contracts.Requests.Catalog;

public class UpdateVariantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal? OverrideCostAmount { get; set; }
    public decimal? OverridePriceAmount { get; set; }
    public string? MainImage { get; set; }
    public List<string> Images { get; set; } = [];
    public Dictionary<Guid, Guid> AttributeValues { get; set; } = new();
}
