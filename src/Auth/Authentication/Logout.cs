using Auth.Services;
using FluentValidation;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authentication;

public class Logout
{
    public record Command(string RefreshToken) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required.");
        }
    }

    public class Handler(IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            await jwtService.RevokeRefreshTokenAsync(command.RefreshToken, ip);
        }
    }

    public record Request(string RefreshToken);

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/logout", async (Request request, ISender sender) =>
            {
                await sender.Send(new Command(request.RefreshToken));
                return Results.Ok();
            })
            .WithName("Logout")
            .WithTags("Authentication");
        }
    }
}
