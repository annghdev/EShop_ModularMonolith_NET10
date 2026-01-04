using FluentValidation;
using Kernel.Application;
using Kernel.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Kernel.Extensions;

public static class ServiceCollectionExtensions
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

        // use UnitOfWork instead, because we can't delete draft entities in background tasks if using this interceptor
        //services.AddScoped<ISaveChangesInterceptor, AuditSaveChangesInterceptor>();
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
        services.AddSingleton<ICacheService, MemoryCacheService>();
        //services.AddScoped<IEmailService, MailkitService>();
        //services.AddScoped<IImageStorageService, CloudinaryService>();

        return services;
    }
}
