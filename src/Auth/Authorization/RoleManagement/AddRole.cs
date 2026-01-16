using FluentValidation;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authorization;

public class AddRole
{
    public record Request(
        string Name,
        string? DisplayName,
        string? Description,
        string? Icon);

    public record Command(Request Request) : ICommand<RoleResponse>
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["roles"];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public record RoleResponse(
        Guid Id,
        string Name,
        string? DisplayName,
        string? Description,
        string? Icon,
        bool IsSystemRole);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(50).WithMessage("Role name must not exceed 50 characters.")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Role name can only contain letters, numbers, and underscores.");
        }
    }

    public class Handler(RoleManager<Role> roleManager)
        : IRequestHandler<Command, RoleResponse>
    {
        public async Task<RoleResponse> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check if role already exists
            if (await roleManager.RoleExistsAsync(command.Request.Name))
            {
                throw new InvalidOperationException($"Role '{command.Request.Name}' already exists.");
            }

            var role = new Role
            {
                Id = Guid.CreateVersion7(),
                Name = command.Request.Name,
                NormalizedName = command.Request.Name.ToUpperInvariant(),
                DisplayName = command.Request.DisplayName ?? command.Request.Name,
                Description = command.Request.Description,
                Icon = command.Request.Icon,
                IsSystemRole = false
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role: {errors}");
            }

            return new RoleResponse(
                role.Id,
                role.Name!,
                role.DisplayName,
                role.Description,
                role.Icon,
                role.IsSystemRole);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/roles", async (Request request, [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(new Command(request));
                return Results.Created($"/auth/roles/{result.Id}", result);
            })
            .WithName("AddRole")
            .WithTags("Authorization")
            .RequireAuthorization("Admin");
        }
    }
}
