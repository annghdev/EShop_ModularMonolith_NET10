using Kernel;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Infrastructure;

public static class RouteBuilderExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointTypes = assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)Activator.CreateInstance(type)!;
            endpoint.Map(app);
        }
    }
}
