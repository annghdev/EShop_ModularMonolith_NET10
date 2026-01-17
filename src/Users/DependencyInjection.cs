using Users.Application;
using Users.Domain;
using Users.Infrastructure;
using Users.Infrastructure.EFCore.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Users;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersContainer(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddSharedKernel(configuration, assembly);

        services.AddDbContext<UsersDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("usersdb"));
        });

        services.AddIntegrationEventHandlers(assembly);

        services.AddScoped<UsersSeeder>();
        services.AddScoped<IUsersUnitOfWork, UsersUnitOfWork>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IGuestRepository, GuestRepository>();

        return services;
    }
}
