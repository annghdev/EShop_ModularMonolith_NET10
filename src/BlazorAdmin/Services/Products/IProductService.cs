using Contracts.Responses.Catalog;
using Contracts.Requests.Catalog;

namespace BlazorAdmin.Services;

public interface IProductService
{
    // Queries
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto?> GetProductBySlugAsync(string slug);
    Task<PaginatedResult<ProductSearchDto>> SearchProductsAsync(
        string? keyword = null,
        string? categoryId = null,
        string? brandId = null,
        string? status = null,
        int page = 1,
        int pageSize = 20);
    
    // Commands  
    Task<Guid> CreateDraftAsync(CreateProductDraftRequest request);
    Task PublishProductAsync(Guid productId);
    Task RepublishProductAsync(Guid productId);
    
    // Attributes Management
    Task AddProductAttributeAsync(string slug, Guid attributeId, int displayOrder, bool hasVariant);
    Task RemoveProductAttributeAsync(string slug, Guid attributeId);
    
    // Variants Management
    Task<Guid> AddVariantAsync(string slug, AddVariantRequest request);
    Task UpdateVariantAsync(string slug, Guid variantId, UpdateVariantRequest request);
    Task RemoveVariantAsync(string slug, Guid variantId);
    
    // Images Management
    Task UpdateThumbnailAsync(string slug, string thumbnailUrl);
    Task AddImageAsync(string slug, string imageUrl);
    Task RemoveImageAsync(string slug, string imageUrl);
    
    // Helper methods for dropdowns
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<List<BrandDto>> GetBrandsAsync();
    Task<List<AttributeDto>> GetAttributesAsync();
}

// PaginatedResult helper class
public class PaginatedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public List<T> Items { get; set; } = [];
    
    public PaginatedResult(int page, int pageSize, List<T> items, int total)
    {
        Page = page;
        PageSize = pageSize;
        Items = items;
        Total = total;
    }
}
