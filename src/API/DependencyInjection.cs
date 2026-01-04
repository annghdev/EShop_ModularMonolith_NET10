using API.Services;
using Kernel.Application;
using Kernel.Extensions;
using Microsoft.EntityFrameworkCore;

namespace API;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUserService>();

        services.AddInfrasServices(configuration);

        return services;
    }
}
