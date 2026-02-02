using Microsoft.AspNetCore.Routing;

namespace Kernel;

public interface IEndpoint
{
    void Map(IEndpointRouteBuilder app);
}