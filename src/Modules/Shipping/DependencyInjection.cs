using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shipping.Application;
using Shipping.Domain;
using Shipping.Infrastructure;
using Shipping.Infrastructure.EFCore.Repositories;
using System.Reflection;

namespace Shipping;

public static class DependencyInjection
{
    public static IServiceCollection AddShippingContainer(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddSharedKernel(configuration, assembly);

        services.AddDbContext<ShippingDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("shippingdb"));
        });

        services.AddScoped<ShippingSeeder>();
        services.AddScoped<IShippingUnitOfWork, ShippingUnitOfWork>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IShippingCarrierRepository, ShippingCarrierRepository>();

        return services;
    }
}
