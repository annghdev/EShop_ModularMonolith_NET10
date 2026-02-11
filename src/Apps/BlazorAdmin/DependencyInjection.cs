using BlazorAdmin.Auth;
using BlazorAdmin.Pages.Settings;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorAdmin;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtParser, JwtParser>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

        // JWT Authorization Handler for API calls
        services.AddTransient<JwtAuthorizationHandler>();

        //services.AddScoped<IProductService, ProductFakeDataService>();

        services.AddHttpClient<ISeedManager, SeedManager>(c => c.BaseAddress = new Uri("http+https://eshop-api"))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        // Product services - HttpClient with JWT auth
        services.AddHttpClient<IProductService, ProductApiService>(c => c.BaseAddress = new Uri("http+https://eshop-api"))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddHttpClient<IAttributeService, AttributeApiService>(c => c.BaseAddress = new Uri("http+https://eshop-api"))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddHttpClient<IBrandService, BrandApiService>(c => c.BaseAddress = new Uri("http+https://eshop-api"))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddHttpClient<ICategoryService, CategoryApiService>(c => c.BaseAddress = new Uri("http+https://eshop-api"))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddHttpClient<IWarehouseService, WarehouseApiService>(c => c.BaseAddress = new Uri("http+https://eshop-api"))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddHttpClient<IInventoryService, InventoryApiService>(c => c.BaseAddress = new Uri("http+https://eshop-api"))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

    }
}
