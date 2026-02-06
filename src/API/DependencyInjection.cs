using API.Services;
using Catalog;
using Inventory;
using Pricing;
using Users;
using ShoppingCart;
using Kernel.Application;
using Microsoft.EntityFrameworkCore;
using Orders;
using Payment;
using Shipping;

namespace API;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();

        services.AddInfrasServices(configuration);


        services.AddInfrasDB(configuration);
        services.AddCatalogContainer(configuration);
        services.AddInventoryContainer(configuration);
        services.AddPricingContainer(configuration);
        services.AddUsersContainer(configuration);
        services.AddShoppingCartContainer(configuration);
        services.AddOrdersContainer(configuration);
        services.AddPaymentContainer(configuration);
        services.AddShippingContainer(configuration);

        return services;
    }
}
