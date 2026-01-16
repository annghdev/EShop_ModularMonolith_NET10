using Auth.Services;
using Contracts;
using FluentValidation;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authentication;

public class Login
{
    public record Command(string Username, string Password) : IRequest<AuthResult>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        }
    }

    public class Handler : IRequestHandler<Command, AuthResult>
    {
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly IJwtService _jwtService;

        public Handler(
            UserManager<Account> userManager,
            SignInManager<Account> signInManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<AuthResult> Handle(Command command, CancellationToken cancellationToken)
        {
            // Find user by username or email
            var account = (await _userManager.FindByNameAsync(command.Username)
                ?? await _userManager.FindByEmailAsync(command.Username))
                ?? throw new UnauthorizedAccessException("Invalid username or password.");

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(account, command.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                throw new UnauthorizedAccessException("Account is locked. Please try again later.");
            }

            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            // Generate tokens
            var tokens = await _jwtService.GenerateTokensAsync(account);

            // Get roles for display
            var roles = await _userManager.GetRolesAsync(account);
            var displayRole = roles.FirstOrDefault() ?? "User";

            return new AuthResult(tokens.AccessToken, tokens.RefreshToken)
            {
                UserInfo = new UserInfo
                {
                    DisplayName = account.UserName ?? account.Email ?? "User",
                    AvatarUrl = account.AvatarUrl ?? string.Empty,
                    DisplayRole = displayRole
                }
            };
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/login", async (Command command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("Login")
            .WithTags("Authentication");
        }
    }
}
