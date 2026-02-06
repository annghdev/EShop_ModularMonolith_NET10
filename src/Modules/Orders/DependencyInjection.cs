using Orders.Application;
using Orders.Domain;
using Orders.Infrastructure;
using Orders.Infrastructure.EFCore.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Orders;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersContainer(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddSharedKernel(configuration, assembly);

        services.AddDbContext<OrdersDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("ordersdb"));
        });

        services.AddScoped<OrdersSeeder>();
        services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
