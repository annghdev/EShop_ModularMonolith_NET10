namespace Contracts.Requests.Catalog;

public class UpdateProductDraftRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal CostAmount { get; set; }
    public decimal PriceAmount { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Depth { get; set; }
    public decimal Weight { get; set; }
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public string? Thumbnail { get; set; }
    public List<string> Images { get; set; } = [];
}
