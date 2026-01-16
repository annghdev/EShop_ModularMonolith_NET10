using Auth.Constants;
using FluentValidation;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Auth.Authorization;

/// <summary>
/// Role Access Control - manage permissions assigned to roles
/// </summary>
public static class RolePermissionControl
{
    #region Assign Permission to Role
    public record AssignPermissionRequest(string Module, string Resource, string Action);

    public record AssignPermissionCommand(
        Guid RoleId,
        string Module,
        string Resource,
        string Action) : IRequest<bool>;

    public class AssignPermissionValidator : AbstractValidator<AssignPermissionCommand>
    {
        public AssignPermissionValidator()
        {
            RuleFor(x => x.RoleId).NotEmpty();
            RuleFor(x => x.Module).NotEmpty();
            RuleFor(x => x.Resource).NotEmpty();
            RuleFor(x => x.Action).NotEmpty();
        }
    }

    public class AssignPermissionHandler(RoleManager<Role> roleManager)
        : IRequestHandler<AssignPermissionCommand, bool>
    {
        public async Task<bool> Handle(AssignPermissionCommand command, CancellationToken cancellationToken)
        {
            var role = await roleManager.FindByIdAsync(command.RoleId.ToString());
            if (role == null)
            {
                throw new KeyNotFoundException($"Role with ID '{command.RoleId}' not found.");
            }

            var permission = Permissions.For(command.Module, command.Resource, command.Action);
            var claim = new Claim(Permissions.ClaimType, permission);

            // Check if already exists
            var existingClaims = await roleManager.GetClaimsAsync(role);
            if (existingClaims.Any(c => c.Type == Permissions.ClaimType && c.Value == permission))
            {
                return true; // Already assigned
            }

            var result = await roleManager.AddClaimAsync(role, claim);
            return result.Succeeded;
        }
    }

    #endregion

    #region Remove Permission from Role

    public record RemovePermissionCommand(
        Guid RoleId,
        string Module,
        string Resource,
        string Action) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [$"role_permissions_{RoleId}"];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public class RemovePermissionValidator : AbstractValidator<RemovePermissionCommand>
    {
        public RemovePermissionValidator()
        {
            RuleFor(x => x.RoleId).NotEmpty();
            RuleFor(x => x.Module).NotEmpty();
            RuleFor(x => x.Resource).NotEmpty();
            RuleFor(x => x.Action).NotEmpty();
        }
    }

    public class RemovePermissionHandler(RoleManager<Role> roleManager)
        : IRequestHandler<RemovePermissionCommand>
    {
        public async Task Handle(RemovePermissionCommand command, CancellationToken cancellationToken)
        {
            var role = await roleManager.FindByIdAsync(command.RoleId.ToString());
            if (role == null)
            {
                throw new NotFoundException("Role", command.RoleId);
            }

            var permission = Permissions.For(command.Module, command.Resource, command.Action);
            var claim = new Claim(Permissions.ClaimType, permission);

            var result = await roleManager.RemoveClaimAsync(role, claim);
        }
    }

    #endregion

    #region Batch Update Permissions

    public record BatchUpdateRequest(List<PermissionUpdateRequest> Permissions);

    public record BatchUpdatePermissionsCommand(Guid RoleId, List<PermissionUpdate> Permissions) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [$"role_permissions_{RoleId}"];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public record PermissionUpdate(
        string Module,
        string Resource,
        string Action,
        bool Granted);

    public class BatchUpdateHandler(RoleManager<Role> roleManager)
        : IRequestHandler<BatchUpdatePermissionsCommand>
    {
        public async Task Handle(BatchUpdatePermissionsCommand command, CancellationToken cancellationToken)
        {
            var role = await roleManager.FindByIdAsync(command.RoleId.ToString());
            if (role == null)
            {
                throw new NotFoundException($"Role", command.RoleId);
            }

            foreach (var update in command.Permissions)
            {
                var permission = Permissions.For(update.Module, update.Resource, update.Action);
                var claim = new Claim(Permissions.ClaimType, permission);

                if (update.Granted)
                {
                    var existingClaims = await roleManager.GetClaimsAsync(role);
                    if (!existingClaims.Any(c => c.Type == Permissions.ClaimType && c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(role, claim);
                    }
                }
                else
                {
                    await roleManager.RemoveClaimAsync(role, claim);
                }
            }
        }
    }

    #endregion

    #region Endpoints

    public class Endpoints : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/auth/roles/{roleId:guid}/permissions")
                .WithTags("Authorization")
                .RequireAuthorization();

            // POST - Assign permission to role
            group.MapPost("", async (Guid roleId, AssignPermissionRequest request, ISender sender) =>
            {
                var command = new AssignPermissionCommand(roleId, request.Module, request.Resource, request.Action);
                await sender.Send(command);
                return Results.Ok();

            }).WithName("AssignPermissionToRole");

            // DELETE - Remove permission from role
            group.MapDelete("{module}/{resource}/{action}", async (
                Guid roleId, string module, string resource, string action, ISender sender) =>
            {
                var command = new RemovePermissionCommand(roleId, module, resource, action);
                await sender.Send(command);
                return Results.NoContent();
            })
            .WithName("RemovePermissionFromRole");

            // PUT - Batch update permissions
            group.MapPut("", async (Guid roleId, BatchUpdateRequest request, ISender sender) =>
            {
                try
                {
                    var permissions = request.Permissions
                        .Select(p => new PermissionUpdate(p.Module, p.Resource, p.Action, p.Granted))
                        .ToList();
                    var command = new BatchUpdatePermissionsCommand(roleId, permissions);
                    await sender.Send(command);
                    return Results.Ok();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            })
            .WithName("BatchUpdateRolePermissions");
        }
    }
    public record PermissionUpdateRequest(string Module, string Resource, string Action, bool Granted);

    #endregion
}
