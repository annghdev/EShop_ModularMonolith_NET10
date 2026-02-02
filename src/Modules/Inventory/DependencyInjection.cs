using Inventory.Application;
using Inventory.Domain;
using Inventory.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Inventory;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryContainer(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddSharedKernel(configuration, assembly);

        services.AddDbContext<InventoryDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("inventorydb"));
        });

        services.AddIntegrationEventHandlers(assembly);

        // Repositories
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();

        // Unit of Work
        services.AddScoped<IInventoryUnitOfWork, InventoryUnitOfWork>();
        services.AddScoped<InventorySeeder>();

        return services;
    }
}
