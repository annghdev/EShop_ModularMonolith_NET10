using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts;
using Contracts.Requests.Inventory;
using Contracts.Responses.Inventory;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorAdmin.Services;

public class InventoryApiService(HttpClient httpClient) : IInventoryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PaginatedResult<StockingProductDto>> GetStockingProductsAsync(
        string? filter,
        Guid? warehouseId,
        bool inStockOnly,
        int page,
        int pageSize)
    {
        var query = new Dictionary<string, string?>
        {
            ["filter"] = filter,
            ["warehouseId"] = warehouseId?.ToString(),
            ["inStockOnly"] = inStockOnly ? "true" : "false",
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        };

        var url = QueryHelpers.AddQueryString("api/admin/inventory/products",
            query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        return await GetAsync<PaginatedResult<StockingProductDto>>(url)
               ?? new PaginatedResult<StockingProductDto>(page, pageSize, [], 0);
    }

    public async Task<ProductQuantityDto?> GetProductInventoryAsync(Guid productId)
    {
        return await GetAsync<ProductQuantityDto>($"api/admin/inventory/products/{productId}");
    }

    public async Task<InventoryItemDto?> GetItemBySkuAsync(string sku, Guid warehouseId)
    {
        var url = QueryHelpers.AddQueryString("api/admin/inventory/items/by-sku", new Dictionary<string, string?>
        {
            ["sku"] = sku,
            ["warehouseId"] = warehouseId.ToString()
        });

        return await GetAsync<InventoryItemDto>(url);
    }

    public async Task AdjustItemQuantityAsync(AdjustVariantQuantityRequest request)
    {
        await PutAsync("api/admin/inventory/items/adjust", request);
    }

    public async Task ImportItemBySkuAsync(ImportItemBySkuRequest request)
    {
        await PostAsync("api/admin/inventory/items/import", request);
    }

    public async Task<PaginatedResult<InventoryMovementDto>> GetInventoryMovementsAsync(
        string? keyword,
        Guid? warehouseId,
        string? type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["keyword"] = keyword,
            ["warehouseId"] = warehouseId?.ToString(),
            ["type"] = type,
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        };

        var url = QueryHelpers.AddQueryString("api/admin/inventory/movements",
            query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        return await GetAsync<PaginatedResult<InventoryMovementDto>>(url, cancellationToken)
               ?? new PaginatedResult<InventoryMovementDto>(page, pageSize, [], 0);
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private async Task PostAsync<T>(string url, T body)
    {
        var response = await httpClient.PostAsJsonAsync(url, body, JsonOptions);
        await EnsureSuccessAsync(response);
    }

    private async Task PutAsync<T>(string url, T body)
    {
        var response = await httpClient.PutAsJsonAsync(url, body, JsonOptions);
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
}
