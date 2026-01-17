using ShoppingCart.Application;
using ShoppingCart.Domain;
using ShoppingCart.Infrastructure;
using ShoppingCart.Infrastructure.EFCore.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ShoppingCart;

public static class DependencyInjection
{
    public static IServiceCollection AddShoppingCartContainer(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddSharedKernel(configuration, assembly);

        services.AddDbContext<ShoppingCartDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("shoppingcartdb"));
        });

        services.AddIntegrationEventHandlers(assembly);

        services.AddScoped<ShoppingCartSeeder>();
        services.AddScoped<IShoppingCartUnitOfWork, ShoppingCartUnitOfWork>();
        services.AddScoped<ICartRepository, CartRepository>();

        return services;
    }
}
