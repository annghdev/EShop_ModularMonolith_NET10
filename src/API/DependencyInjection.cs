using API.Services;
using Catalog;
using Inventory;
using Pricing;
using Users;
using Kernel.Application;
using Microsoft.EntityFrameworkCore;

namespace API;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUserService>();

        services.AddInfrasServices(configuration);
        services.AddMassTransitWithHandlers(
            configuration,
            typeof(Catalog.DependencyInjection).Assembly,
            typeof(Inventory.DependencyInjection).Assembly,
            typeof(Pricing.DependencyInjection).Assembly,
            typeof(Users.DependencyInjection).Assembly);

        //services.AddInfrasDB(configuration);
        services.AddCatalogContainer(configuration);
        services.AddInventoryContainer(configuration);
        services.AddPricingContainer(configuration);
        services.AddUsersContainer(configuration);

        return services;
    }
}
