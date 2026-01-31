using BlazorAdmin.Auth;
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

        // Product services - HttpClient with JWT auth
        services.AddHttpClient<IProductService, ProductApiService>(c => c.BaseAddress = new Uri("http+https://api"))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();
    }
}
