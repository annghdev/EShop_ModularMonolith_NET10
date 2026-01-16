using Auth.Authorization.DTOs;
using Auth.Constants;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authorization.PermissionManagement;

public class GetUserPermissions
{
    public record GetPermissionsQuery(Guid UserId, bool IncludeRolePermissions = true)
        : IQuery<UserPermissionsResponse>
    {
        public string CacheKey => $"user_permissions_{UserId}";

        public TimeSpan? ExpirationSliding => TimeSpan.FromDays(7);
    }

    public record UserPermissionsResponse
    {
        public PermissionMatrixResponse DirectPermissions { get; init; } = new();
        public PermissionMatrixResponse? EffectivePermissions { get; init; } // Combined role + direct
        public List<string> Roles { get; init; } = [];
    }

    public class GetPermissionsHandler(UserManager<Account> userManager, RoleManager<Role> roleManager)
        : IRequestHandler<GetPermissionsQuery, UserPermissionsResponse>
    {
        public async Task<UserPermissionsResponse> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(query.UserId.ToString())
                ?? throw new NotFoundException("User", query.UserId);

            // Get direct user claims
            var userClaims = await userManager.GetClaimsAsync(user);
            var directPermissions = userClaims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();

            // Get roles
            var roles = (await userManager.GetRolesAsync(user)).ToList();

            // Build direct permissions matrix
            var directMatrix = BuildPermissionMatrix(directPermissions);

            UserPermissionsResponse response;

            if (query.IncludeRolePermissions)
            {
                // Get all role permissions
                var effectivePermissions = new HashSet<string>(directPermissions);
                foreach (var roleName in roles)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        var roleClaims = await roleManager.GetClaimsAsync(role);
                        foreach (var claim in roleClaims.Where(c => c.Type == Permissions.ClaimType))
                        {
                            effectivePermissions.Add(claim.Value);
                        }
                    }
                }

                response = new UserPermissionsResponse
                {
                    DirectPermissions = directMatrix,
                    EffectivePermissions = BuildPermissionMatrix(effectivePermissions),
                    Roles = roles
                };
            }
            else
            {
                response = new UserPermissionsResponse
                {
                    DirectPermissions = directMatrix,
                    Roles = roles
                };
            }

            return response;
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("", async (Guid userId, bool includeRolePermissions, ISender sender) =>
            {
                var result = await sender.Send(new GetPermissionsQuery(userId, includeRolePermissions));
                return Results.Ok(result);
            })
            .WithName("GetUserPermissions")
            .WithTags("Authorization");
        }
    }

    private static PermissionMatrixResponse BuildPermissionMatrix(HashSet<string> grantedPermissions)
    {
        var modules = new List<ModulePermissions>();

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
