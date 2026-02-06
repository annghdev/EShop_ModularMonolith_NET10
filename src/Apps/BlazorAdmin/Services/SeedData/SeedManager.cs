namespace BlazorAdmin.Services;

public class SeedManager(HttpClient httpClient) : ISeedManager
{
    public async Task SeedProducts()
    {
        var response = await httpClient.PostAsync("api/products/create-sample-data", null);
        response.EnsureSuccessStatusCode();
    }
}
