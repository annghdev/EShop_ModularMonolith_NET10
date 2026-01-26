namespace Contracts.Responses;

public class ProductSearchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public MoneyDto? Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
    public string Status { get; set; } = string.Empty;
    public int VariantCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
