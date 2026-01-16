using FluentValidation;
using Kernel.Application;
using Kernel.Extensions;
using MassTransit;
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
        services.AddScoped<IIntegrationEventPublisher, MasstransitEventPublisher>();

        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IEmailService, MailkitService>();
        //services.AddScoped<IImageStorageService, CloudinaryService>();

        return services;
    }

    public static IServiceCollection AddMassTransitWithHandlers(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var handlerInterface = typeof(IIntegrationEventHandler<>);
        var assemblyMessageTypes = assemblies
            .Select(assembly => new
            {
                Assembly = assembly,
                MessageTypes = assembly.GetTypes()
                    .SelectMany(t => t.GetInterfaces()
                        .Where(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == handlerInterface)
                        .Select(i => i.GenericTypeArguments[0]))
                    .Distinct()
                    .ToList()
            })
            .Where(x => x.MessageTypes.Count > 0)
            .ToList();

        services.AddMassTransit(x =>
        {
            foreach (var msgType in assemblyMessageTypes.SelectMany(x => x.MessageTypes).Distinct())
            {
                var consumerType = typeof(MassTransitEventHandler<>).MakeGenericType(msgType);
                x.AddConsumer(consumerType);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("rabbitmq"));

                foreach (var assemblyInfo in assemblyMessageTypes)
                {
                    var serviceName = assemblyInfo.Assembly.GetName().Name!.ToKebabCase();

                    foreach (var msgType in assemblyInfo.MessageTypes)
                    {
                        var queueName = $"{msgType.Name.ToKebabCase()}.{serviceName}";

                        cfg.ReceiveEndpoint(queueName, e =>
                        {
                            var consumerType = typeof(MassTransitEventHandler<>).MakeGenericType(msgType);
                            e.ConfigureConsumer(context, consumerType);
                        });
                    }
                }
            });
        });

        return services;
    }

    public static void AddIntegrationEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IIntegrationEventHandler<>);

        var handlers = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(type => type.GetInterfaces(), (type, iface) => new { type, iface })
            .Where(x =>
                x.iface.IsGenericType &&
                x.iface.GetGenericTypeDefinition() == handlerInterface
            );

        foreach (var c in handlers)
        {
            services.AddScoped(c.iface, c.type);
        }
    }
}



