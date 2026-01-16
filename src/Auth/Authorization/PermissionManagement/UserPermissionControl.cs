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
/// User Access Control - manage permissions assigned directly to users (overrides role permissions)
/// </summary>
public static class UserPermissionControl
{
    #region Assign Permission to User

    public record AssignPermissionRequest(string Module, string Resource, string Action);

    public record AssignPermissionCommand(
        Guid UserId,
        string Module,
        string Resource,
        string Action) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [$"user_permission_{UserId}"];

        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public class AssignPermissionValidator : AbstractValidator<AssignPermissionCommand>
    {
        public AssignPermissionValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Module).NotEmpty();
            RuleFor(x => x.Resource).NotEmpty();
            RuleFor(x => x.Action).NotEmpty();
        }
    }

    public class AssignPermissionHandler(UserManager<Account> userManager)
        : IRequestHandler<AssignPermissionCommand>
    {
        public async Task Handle(AssignPermissionCommand command, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null)
            {
                throw new NotFoundException("User", command.UserId);
            }

            var permission = Permissions.For(command.Module, command.Resource, command.Action);
            var claim = new Claim(Permissions.ClaimType, permission);

            // Check if already exists
            var existingClaims = await userManager.GetClaimsAsync(user);
            if (existingClaims.Any(c => c.Type == Permissions.ClaimType && c.Value == permission))
            {
                return;
            }

            var result = await userManager.AddClaimAsync(user, claim);
        }
    }

    #endregion

    #region Remove Permission from User

    public record RemovePermissionCommand(
        Guid UserId,
        string Module,
        string Resource,
        string Action) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [$"user_permission_{UserId}"];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public class RemovePermissionValidator : AbstractValidator<RemovePermissionCommand>
    {
        public RemovePermissionValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Module).NotEmpty();
            RuleFor(x => x.Resource).NotEmpty();
            RuleFor(x => x.Action).NotEmpty();
        }
    }

    public class RemovePermissionHandler : IRequestHandler<RemovePermissionCommand>
    {
        private readonly UserManager<Account> _userManager;

        public RemovePermissionHandler(UserManager<Account> userManager)
        {
            _userManager = userManager;
        }

        public async Task Handle(RemovePermissionCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null)
            {
                throw new NotFoundException("User", command.UserId);
            }

            var permission = Permissions.For(command.Module, command.Resource, command.Action);
            var claim = new Claim(Permissions.ClaimType, permission);

            await _userManager.RemoveClaimAsync(user, claim);
        }
    }

    #endregion

    #region Batch Update User Permissions

    public record BatchUpdateRequest(List<PermissionUpdateRequest> Permissions);
    public record PermissionUpdateRequest(string Module, string Resource, string Action, bool Granted);

    public record BatchUpdatePermissionsCommand(
        Guid UserId,
        List<PermissionUpdate> Permissions) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [$"user_permission_{UserId}"];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public record PermissionUpdate(
        string Module,
        string Resource,
        string Action,
        bool Granted);

    public class BatchUpdateHandler : IRequestHandler<BatchUpdatePermissionsCommand>
    {
        private readonly UserManager<Account> _userManager;

        public BatchUpdateHandler(UserManager<Account> userManager)
        {
            _userManager = userManager;
        }

        public async Task Handle(BatchUpdatePermissionsCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null)
            {
                throw new NotFoundException("User", command.UserId);
            }

            foreach (var update in command.Permissions)
            {
                var permission = Permissions.For(update.Module, update.Resource, update.Action);
                var claim = new Claim(Permissions.ClaimType, permission);

                if (update.Granted)
                {
                    var existingClaims = await _userManager.GetClaimsAsync(user);
                    if (!existingClaims.Any(c => c.Type == Permissions.ClaimType && c.Value == permission))
                    {
                        await _userManager.AddClaimAsync(user, claim);
                    }
                }
                else
                {
                    await _userManager.RemoveClaimAsync(user, claim);
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
            var group = app.MapGroup("/auth/users/{userId:guid}/permissions")
                .WithTags("User Authorization")
                .RequireAuthorization();

            // POST - Assign permission to user
            group.MapPost("", async (Guid userId, AssignPermissionRequest request, ISender sender) =>
            {
                try
                {
                    var command = new AssignPermissionCommand(userId, request.Module, request.Resource, request.Action);
                    await sender.Send(command);
                    return Results.Ok();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            }).WithName("AssignPermissionToUser");

            // DELETE - Remove permission from user
            group.MapDelete("{module}/{resource}/{action}", async (
                Guid userId, string module, string resource, string action, ISender sender) =>
            {
                try
                {
                    var command = new RemovePermissionCommand(userId, module, resource, action);
                    await sender.Send(command);
                    return Results.NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            }).WithName("RemovePermissionFromUser");

            // PUT - Batch update permissions
            group.MapPut("", async (Guid userId, BatchUpdateRequest request, ISender sender) =>
            {
                try
                {
                    var permissions = request.Permissions
                        .Select(p => new PermissionUpdate(p.Module, p.Resource, p.Action, p.Granted))
                        .ToList();
                    var command = new BatchUpdatePermissionsCommand(userId, permissions);
                    await sender.Send(command);
                    return Results.Ok();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            }).WithName("BatchUpdateUserPermissions");
        }
    }

    #endregion
}
