using Pricing.Application;
using Pricing.Domain;
using Pricing.Infrastructure;
using Pricing.Infrastructure.EFCore.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Pricing;

public static class DependencyInjection
{
    public static IServiceCollection AddPricingContainer(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddSharedKernel(configuration, assembly);

        services.AddDbContext<PricingDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("pricingdb"));
        });

        services.AddIntegrationEventHandlers(assembly);

        services.AddScoped<PricingSeeder>();
        services.AddScoped<IPricingUnitOfWork, PricingUnitOfWork>();
        services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();

        return services;
    }
}
