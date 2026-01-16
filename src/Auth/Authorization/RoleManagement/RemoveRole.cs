using FluentValidation;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authorization;

public class RemoveRole
{
    public record Command(Guid RoleId) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["roles"];

        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.");
        }
    }

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var role = await roleManager.FindByIdAsync(command.RoleId.ToString());
            if (role == null)
            {
                throw new NotFoundException("Role", command.RoleId.ToString());
            }

            // Prevent deletion of system roles
            if (role.IsSystemRole)
            {
                throw new DomainException($"Cannot delete system role '{role.Name}'.");
            }

            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to delete role: {errors}");
            }
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/auth/roles/{roleId:guid}", async (Guid roleId, ISender sender) =>
            {
                await sender.Send(new Command(roleId));
                return Results.NoContent();
            })
            .WithName("RemoveRole")
            .WithTags("Authorization")
            .RequireAuthorization("Admin");
        }
    }
}
