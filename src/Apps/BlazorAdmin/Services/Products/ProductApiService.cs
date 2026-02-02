using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts;
using Contracts.Requests.Catalog;
using Contracts.Responses;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorAdmin.Services;

/// <summary>
/// HTTP API client implementation of IProductService with explicit error handling.
/// </summary>
public class ProductApiService(HttpClient httpClient) : IProductService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    #region Queries

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        return await GetAsync<ProductDto>($"api/products/{id}");
    }

    public async Task<ProductDto?> GetProductBySlugAsync(string slug)
    {
        return await GetAsync<ProductDto>($"api/products/by-slug/{slug}");
    }

    public async Task<PaginatedResult<ProductSearchDto>> SearchProductsAsync(
        string? keyword = null,
        string? categoryId = null,
        string? brandId = null,
        string? status = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = new Dictionary<string, string?>
        {
            ["keyword"] = keyword,
            ["categoryId"] = categoryId,
            ["brandId"] = brandId,
            ["status"] = status,
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["sortBy"] = "createdAt",
            ["sortOrder"] = "desc"
        };

        var url = QueryHelpers.AddQueryString("api/admin/products",
            query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        return await GetAsync<PaginatedResult<ProductSearchDto>>(url) ?? new PaginatedResult<ProductSearchDto>(page, pageSize, [], 0);
    }

    public async Task<List<ProductSearchDto>> GetDraftsAsync()
    {
        return await GetAsync<List<ProductSearchDto>>("api/admin/products/drafts") ?? [];
    }

    public async Task<List<CategoryDefaultAttributeDto>> GetCategoryDefaultAttributesAsync(Guid categoryId)
    {
        return await GetAsync<List<CategoryDefaultAttributeDto>>($"api/categories/{categoryId}/default-attributes") ?? [];
    }

    #endregion

    #region Commands

    public async Task<Guid> CreateDraftAsync(CreateProductDraftRequest request)
    {
        await PostAsync("api/products", request);
        return request.Id;
    }

    public async Task PublishProductAsync(Guid productId)
    {
        await PutAsync($"api/products/publish/{productId}");
    }

    public async Task RepublishProductAsync(Guid productId)
    {
        await PutAsync($"api/products/{productId}/republish");
    }

    public async Task UpdateDraftAsync(Guid productId, UpdateProductDraftRequest request)
    {
        await PutAsync($"api/admin/products/{productId}/draft", request);
    }

    public async Task DiscardDraftAsync(Guid productId)
    {
        await DeleteAsync($"api/products/{productId}/discard");
    }

    public async Task UpdateProductPricingAsync(Guid productId, decimal costAmount, decimal priceAmount)
    {
        var request = new UpdateProductPricingRequest(
            new MoneyDto(costAmount, "VND"),
            new MoneyDto(priceAmount, "VND")
        );

        await PutAsync($"api/products/{productId}/pricing", request);
    }

    /// <summary>
    /// Creates a new empty draft product.
    /// Loads default category and brand, then creates a minimal draft.
    /// </summary>
    public async Task<ProductDto> CreateNewDraftAsync()
    {
        var categories = await GetCategoriesAsync();
        var brands = await GetBrandsAsync();

        var defaultCategory = categories.FirstOrDefault();
        var defaultBrand = brands.FirstOrDefault();

        if (defaultCategory == null || defaultBrand == null)
        {
            throw new InvalidOperationException("Không thể tạo bản nháp: chưa có danh mục hoặc thương hiệu.");
        }

        var newId = Guid.CreateVersion7();
        var timestamp = GetVietnamNow().ToString("yyyyMMddHHmmss");

        var request = new CreateProductDraftRequest
        {
            Id = newId,
            Name = "New Product Draft",
            Sku = $"DRAFT-{timestamp}",
            Cost = new MoneyDto(0, "VND"),
            Price = new MoneyDto(0, "VND"),
            Dimensions = new DimensionsDto(1, 1, 1, 1),
            HasStockQuantity = true,
            CategoryId = defaultCategory.Id,
            BrandId = defaultBrand.Id
            // No variants - user will add them from the UI
        };

        await PostAsync("api/products", request);

        var createdProduct = await GetProductByIdAsync(newId);
        if (createdProduct != null)
        {
            return createdProduct;
        }

        return new ProductDto
        {
            Id = newId,
            Name = request.Name,
            Sku = request.Sku,
            Status = "Draft",
            Category = defaultCategory,
            Brand = defaultBrand
        };
    }

    #endregion

    #region Attributes Management

    public async Task AddProductAttributeAsync(string slug, Guid attributeId, int displayOrder, bool hasVariant)
    {
        await PostAsync($"api/products/{slug}/attributes", new AddProductAttributeApiRequest(attributeId, displayOrder, hasVariant));
    }

    public async Task RemoveProductAttributeAsync(string slug, Guid attributeId)
    {
        await DeleteAsync($"api/products/{slug}/attributes/{attributeId}");
    }

    public async Task UpdateProductAttributeAsync(string slug, Guid attributeId, bool hasVariant)
    {
        await PutAsync($"api/products/{slug}/attributes/{attributeId}", new UpdateProductAttributeApiRequest(hasVariant));
    }

    #endregion

    #region Variants Management

    public async Task<Guid> AddVariantAsync(Guid productId, AddVariantRequest request)
    {
        var apiRequest = new AddVariantApiRequest
        {
            ProductId = productId,
            Name = request.Name,
            Sku = request.Sku,
            OverrideCost = request.OverrideCostAmount.HasValue ? new MoneyDto(request.OverrideCostAmount.Value, "VND") : null,
            OverridePrice = request.OverridePriceAmount.HasValue ? new MoneyDto(request.OverridePriceAmount.Value, "VND") : null,
            Dimensions = null,
            MainImage = request.MainImage,
            Images = request.Images,
            AttributeValues = request.AttributeValues
        };

        await PostAsync($"api/products/{productId}/variants", apiRequest);
        return productId;
    }

    public async Task UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantRequest request)
    {
        var apiRequest = new UpdateVariantApiRequest
        {
            ProductId = productId,
            VariantId = variantId,
            Name = request.Name,
            Sku = request.Sku,
            OverrideCost = request.OverrideCostAmount.HasValue ? new MoneyDto(request.OverrideCostAmount.Value, "VND") : null,
            OverridePrice = request.OverridePriceAmount.HasValue ? new MoneyDto(request.OverridePriceAmount.Value, "VND") : null,
            Dimensions = null,
            MainImage = request.MainImage,
            Images = request.Images,
            AttributeValues = request.AttributeValues
        };

        await PutAsync($"api/products/{productId}/variants/{variantId}", apiRequest);
    }

    public async Task RemoveVariantAsync(Guid productId, Guid variantId)
    {
        await DeleteAsync($"api/products/{productId}/variants/{variantId}");
    }

    #endregion

    #region Images Management

    public async Task UpdateThumbnailAsync(string slug, string thumbnailUrl)
    {
        await PutAsync($"api/products/{slug}/thumbnail", new UpdateThumbnailApiRequest(thumbnailUrl));
    }

    public async Task AddImageAsync(string slug, string imageUrl)
    {
        await PostAsync($"api/products/{slug}/images", new AddImageApiRequest(imageUrl));
    }

    public async Task RemoveImageAsync(string slug, string imageUrl)
    {
        await DeleteAsync($"api/products/{slug}/images?imageUrl={Uri.EscapeDataString(imageUrl)}");
    }

    #endregion

    #region Helper Methods

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await GetAsync<List<CategoryDto>>("api/categories") ?? [];
    }

    public async Task<List<BrandDto>> GetBrandsAsync()
    {
        return await GetAsync<List<BrandDto>>("api/brands") ?? [];
    }

    public async Task<List<AttributeDto>> GetAttributesAsync()
    {
        return await GetAsync<List<AttributeDto>>("api/attributes") ?? [];
    }

    #endregion

    #region Internal Helpers

    private async Task<T?> GetAsync<T>(string url)
    {
        var response = await httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task PostAsync<T>(string url, T body)
    {
        var response = await httpClient.PostAsJsonAsync(url, body, JsonOptions);
        await EnsureSuccessAsync(response);
    }

    private async Task PutAsync(string url)
    {
        var response = await httpClient.PutAsync(url, null);
        await EnsureSuccessAsync(response);
    }

    private async Task PutAsync<T>(string url, T body)
    {
        var response = await httpClient.PutAsJsonAsync(url, body, JsonOptions);
        await EnsureSuccessAsync(response);
    }

    private async Task DeleteAsync(string url)
    {
        var response = await httpClient.DeleteAsync(url);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        var message = ExtractErrorMessage(content);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = $"Yêu cầu thất bại với mã {(int)response.StatusCode}.";
        }

        throw new InvalidOperationException(message);
    }

    private static string ExtractErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? content;
            }

            if (doc.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? content;
            }

            if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in errors.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array && prop.Value.GetArrayLength() > 0)
                    {
                        return prop.Value[0].GetString() ?? content;
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Ignore JSON parse errors, fallback to raw content
        }

        return content;
    }

    private static DateTimeOffset GetVietnamNow()
    {
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vnTimeZone);
    }

    private sealed class AddVariantApiRequest
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public MoneyDto? OverrideCost { get; set; }
        public MoneyDto? OverridePrice { get; set; }
        public DimensionsDto? Dimensions { get; set; }
        public string? MainImage { get; set; }
        public IEnumerable<string> Images { get; set; } = [];
        public Dictionary<Guid, Guid> AttributeValues { get; set; } = new();
    }

    private sealed class UpdateVariantApiRequest
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public MoneyDto? OverrideCost { get; set; }
        public MoneyDto? OverridePrice { get; set; }
        public DimensionsDto? Dimensions { get; set; }
        public string? MainImage { get; set; }
        public IEnumerable<string> Images { get; set; } = [];
        public Dictionary<Guid, Guid> AttributeValues { get; set; } = new();
    }

    private sealed record UpdateProductAttributeApiRequest(bool HasVariant);
    private sealed record UpdateProductPricingRequest(MoneyDto Cost, MoneyDto Price);
    public record AddProductAttributeApiRequest(Guid AttributeId, int DisplayOrder, bool HasVariant = false);
    public record UpdateThumbnailApiRequest(string ThumbnailUrl);
    public record AddImageApiRequest(string ImageUrl);

    #endregion
}
