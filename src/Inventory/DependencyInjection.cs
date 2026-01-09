using Inventory.Application;
using Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;
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

        services.AddScoped<IInventoryUnitOfWork, InventoryUnitOfWork>();

        return services;
    }
}
