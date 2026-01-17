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

        services.AddScoped<ICurrentUser, CurrentUserService>();

        services.AddInfrasServices(configuration);
        services.AddMassTransitWithHandlers(
            configuration,
            typeof(Auth.DependencyInjection).Assembly,
            typeof(Users.DependencyInjection).Assembly,
            typeof(Catalog.DependencyInjection).Assembly,
            typeof(Inventory.DependencyInjection).Assembly,
            typeof(Pricing.DependencyInjection).Assembly,
            typeof(Orders.DependencyInjection).Assembly,
            typeof(Payment.DependencyInjection).Assembly,
            typeof(ShoppingCart.DependencyInjection).Assembly,
            typeof(Shipping.DependencyInjection).Assembly);

        //services.AddInfrasDB(configuration);
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
