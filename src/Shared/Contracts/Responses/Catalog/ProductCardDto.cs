namespace Contracts.Responses;

/// <summary>
/// DTO for ProductCard UI display
/// </summary>
public class ProductCardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;

    // Pricing
    public decimal OriginalPrice { get; set; }
    public int DiscountPercent { get; set; }
    public decimal DiscountedPrice { get; set; }
    public string Currency { get; set; } = "VND";

    // Rating & Stats (hardcoded for demo)
    public decimal Rating { get; set; }
    public int SoldCount { get; set; }
    public int FeedbackCount { get; set; }

    // Tags
    public string FeaturedTag { get; set; } = "None"; // Hot/New/None

    // Images
    public string? Thumbnail { get; set; }
    public string? SecondaryImage { get; set; }

    // Variant dots - attributes with HasVariant = true
    public List<VariantDotDto> VariantDots { get; set; } = [];
}

/// <summary>
/// Represents an attribute group for variant display (e.g., Color, RAM)
/// </summary>
public class VariantDotDto
{
    public string AttributeName { get; set; } = string.Empty;
    public string DisplayType { get; set; } = string.Empty; // "color", "text"
    public string? ValueStyleCss { get; set; }
    public List<VariantDotValueDto> Values { get; set; } = [];
}

/// <summary>
/// Represents a single variant option value
/// </summary>
public class VariantDotValueDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
}
