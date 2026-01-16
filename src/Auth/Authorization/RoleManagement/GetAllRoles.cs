using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authorization.RoleManagement;

public class GetAllRoles
{
    public class Query : IQuery<IEnumerable<RoleDto>>
    {
        public string CacheKey => "roles";
        public TimeSpan? ExpirationSliding => TimeSpan.FromDays(7);
    }

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Query, IEnumerable<RoleDto>>
    {
        public async Task<IEnumerable<RoleDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var roles = await roleManager.Roles.ToListAsync(cancellationToken);
            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                DisplayName = r.DisplayName,
                Description = r.Description,
                Icon = r.Icon
            }).ToList();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("auth/roles", async (ISender sender) =>
            {
                var result = await sender.Send(new Query()) ?? [];
                return Results.Ok(result);
            })
            .WithName("GetRoles")
            .WithTags("Auth")
            .RequireAuthorization("Admin");
        }
    }
}
