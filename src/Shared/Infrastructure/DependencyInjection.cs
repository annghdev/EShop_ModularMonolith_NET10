using FluentValidation;
using Kernel.Application;
using Kernel.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
        });
        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    public static IServiceCollection AddInfrasDB(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InfrasDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("infrasdb"));
        });

        return services;
    }

    public static IServiceCollection AddInfrasServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IIntegrationRequestSender, IntegrationRequestSender>();
        services.AddScoped<IIntegrationEventPublisher, WolverineEventPublisher>();

        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IEmailService, MailkitService>();
        services.AddScoped<IImageStorageService, CloudinaryService>();

        return services;
    }
}



