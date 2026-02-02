using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Domain;
using Payment.Infrastructure;
using Payment.Infrastructure.EFCore.Repositories;
using System.Reflection;

namespace Payment;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentContainer(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddSharedKernel(configuration, assembly);

        services.AddDbContext<PaymentDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("paymentdb"));
        });

        services.AddIntegrationEventHandlers(assembly);

        services.AddScoped<PaymentSeeder>();
        services.AddScoped<IPaymentUnitOfWork, PaymentUnitOfWork>();
        services.AddScoped<IPaymentGatewayRepository, PaymentGatewayRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

        return services;
    }
}
