using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts.Requests.Inventory;
using Contracts.Responses.Inventory;

namespace BlazorAdmin.Services;

public class WarehouseApiService(HttpClient httpClient) : IWarehouseService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<WarehouseDto>> GetAllAsync()
    {
        return await GetAsync<List<WarehouseDto>>("api/admin/inventory/warehouses") ?? [];
    }

    public async Task<List<WarehouseSimpleDto>> GetActiveAsync()
    {
        return await GetAsync<List<WarehouseSimpleDto>>("api/admin/inventory/warehouses/active") ?? [];
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id)
    {
        return await GetAsync<WarehouseDto>($"api/admin/inventory/warehouses/{id}");
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request)
    {
        var response = await PostAsync<WarehouseDto>("api/admin/inventory/warehouses", request);
        return response ?? throw new InvalidOperationException("Không thể tạo kho.");
    }

    public async Task UpdateAsync(Guid id, UpdateWarehouseRequest request)
    {
        await PutAsync($"api/admin/inventory/warehouses/{id}", request);
    }

    public async Task DeleteAsync(Guid id)
    {
        await DeleteAsync($"api/admin/inventory/warehouses/{id}");
    }

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

    private async Task<T?> PostAsync<T>(string url, object body)
    {
        var response = await httpClient.PostAsJsonAsync(url, body, JsonOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task PutAsync(string url, object body)
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
}
