using Auth.Authorization.DTOs;
using Auth.Constants;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authorization;

public class GetRolePermissions
{
    public record GetPermissionsQuery(Guid RoleId) : IQuery<PermissionMatrixResponse>
    {
        public string CacheKey => $"role_permissions_{RoleId}";
        public TimeSpan? ExpirationSliding => TimeSpan.FromDays(7);
    }

    public class GetPermissionsHandler(RoleManager<Role> roleManager)
        : IRequestHandler<GetPermissionsQuery, PermissionMatrixResponse>
    {
        public async Task<PermissionMatrixResponse> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
        {
            var role = await roleManager.FindByIdAsync(query.RoleId.ToString())
                ?? throw new NotFoundException("Role", query.RoleId.ToString());

            var claims = await roleManager.GetClaimsAsync(role);
            var permissionClaims = claims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();

            return BuildPermissionMatrix(permissionClaims);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // GET - Get role permissions matrix
            app.MapGet("", async (Guid roleId, ISender sender) =>
            {
                var result = await sender.Send(new GetPermissionsQuery(roleId));
                return Results.Ok(result);
            })
            .WithName("GetRolePermissions")
            .WithTags("Authorization");
        }
    }

    private static PermissionMatrixResponse BuildPermissionMatrix(HashSet<string> grantedPermissions)
    {
        var modules = new List<ModulePermissions>();

        // Build matrix for each module
        var moduleResourceMap = new Dictionary<string, string[]>
        {
            [PermissionModules.Auth] = PermissionResources.Auth.All,
            [PermissionModules.Users] = PermissionResources.Users.All,
            [PermissionModules.Catalog] = PermissionResources.Catalog.All,
            [PermissionModules.Inventory] = PermissionResources.Inventory.All,
            [PermissionModules.Pricing] = PermissionResources.Pricing.All,
            [PermissionModules.Orders] = PermissionResources.Orders.All,
            [PermissionModules.Payment] = PermissionResources.Payment.All,
            [PermissionModules.Shipping] = PermissionResources.Shipping.All,
            [PermissionModules.Report] = PermissionResources.Report.All
        };

        foreach (var (moduleName, resources) in moduleResourceMap)
        {
            var resourcePermissions = new List<ResourcePermissions>();

            foreach (var resource in resources)
            {
                var actions = new Dictionary<string, bool>();
                foreach (var action in PermissionActions.All)
                {
                    var permission = Permissions.For(moduleName, resource, action);
                    actions[action] = grantedPermissions.Contains(permission);
                }

                resourcePermissions.Add(new ResourcePermissions
                {
                    ResourceName = resource,
                    Actions = actions
                });
            }

            modules.Add(new ModulePermissions
            {
                ModuleName = moduleName,
                Resources = resourcePermissions
            });
        }

        return new PermissionMatrixResponse { Modules = modules };
    }
}
