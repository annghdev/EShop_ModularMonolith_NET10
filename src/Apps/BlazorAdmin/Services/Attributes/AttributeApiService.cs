using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts.Requests.Catalog;
using Contracts.Responses;

namespace BlazorAdmin.Services;

public class AttributeApiService(HttpClient httpClient) : IAttributeService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<AttributeDto>> GetAttributesAsync()
    {
        return await GetAsync<List<AttributeDto>>("api/attributes") ?? [];
    }

    public async Task<AttributeDto?> GetAttributeByIdAsync(Guid id)
    {
        return await GetAsync<AttributeDto>($"api/attributes/{id}");
    }

    public async Task<Guid> CreateAttributeAsync(CreateAttributeRequest request)
    {
        var response = await PostAsync("api/attributes", request);
        var payload = await response.Content.ReadFromJsonAsync<IdResponse>(JsonOptions);
        return payload?.Id ?? Guid.Empty;
    }

    public async Task UpdateAttributeAsync(Guid id, UpdateAttributeRequest request)
    {
        await PutAsync($"api/attributes/{id}", request);
    }

    public async Task DeleteAttributeAsync(Guid id)
    {
        await DeleteAsync($"api/attributes/{id}");
    }

    public async Task<Guid> AddAttributeValueAsync(Guid attributeId, AddAttributeValueRequest request)
    {
        var response = await PostAsync($"api/attributes/{attributeId}/values", request);
        var payload = await response.Content.ReadFromJsonAsync<IdResponse>(JsonOptions);
        return payload?.Id ?? Guid.Empty;
    }

    public async Task UpdateAttributeValueAsync(Guid attributeId, Guid valueId, UpdateAttributeValueRequest request)
    {
        await PutAsync($"api/attributes/{attributeId}/values/{valueId}", request);
    }

    public async Task RemoveAttributeValueAsync(Guid attributeId, Guid valueId)
    {
        await DeleteAsync($"api/attributes/{attributeId}/values/{valueId}");
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

    private async Task<HttpResponseMessage> PostAsync<T>(string url, T body)
    {
        var response = await httpClient.PostAsJsonAsync(url, body, JsonOptions);
        await EnsureSuccessAsync(response);
        return response;
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
            // ignore
        }

        return content;
    }

    private sealed class IdResponse
    {
        public Guid Id { get; set; }
    }
}
