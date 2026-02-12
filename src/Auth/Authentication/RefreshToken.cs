using Auth.Services;
using Contracts;
using FluentValidation;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authentication;

public class RefreshToken
{
    public record Command(string RefreshTokenValue) : IRequest<TokensResult>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RefreshTokenValue)
                .NotEmpty()
                .WithMessage("Refresh token is required.");
        }
    }

    public class Handler(IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, TokensResult>
    {
        public async Task<TokensResult> Handle(Command command, CancellationToken cancellationToken)
        {
            var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return await jwtService.RefreshTokensAsync(command.RefreshTokenValue, ip);
        }
    }

    public record Request(string RefreshToken);

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/refresh", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new Command(request.RefreshToken));
                return Results.Ok(result);
            })
            .WithName("RefreshToken")
            .WithTags("Authentication");
        }
    }
}
