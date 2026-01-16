using Auth.Constants;
using Auth.Services;
using Contracts;
using FluentValidation;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Auth.Authentication;

public class RegisterAccountForCustomer
{
    public record Command(
        string Email,
        string Password,
        string? PhoneNumber,
        string? FullName,
        string? GuestId) : IRequest<AuthResult>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        }
    }

    public class Handler : IRequestHandler<Command, AuthResult>
    {
        private readonly UserManager<Account> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public Handler(
            UserManager<Account> userManager,
            IJwtService jwtService,
            IIntegrationEventPublisher eventPublisher)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _eventPublisher = eventPublisher;
        }

        public async Task<AuthResult> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(command.Email);
            if (existingUser != null)
            {
                throw new DomainException("An account with this email already exists.");
            }

            // Create new account
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserName = command.Email, // Use email as username
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                CreatedAt = DateTimeOffset.UtcNow,
                EmailConfirmed = false // Require email confirmation
            };

            var result = await _userManager.CreateAsync(account, command.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create account: {errors}");
            }

            // Add Customer role
            await _userManager.AddToRoleAsync(account, DefaultRoles.Customer);

            // Publish integration event
            var integrationEvent = new AccountRegiteredForcustomerIntegrationEvent(
                AccountId: account.Id,
                GuestId: command.GuestId,
                FullName: command.FullName,
                Email: command.Email,
                PhoneNumber: command.PhoneNumber);

            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

            // Generate tokens
            var tokens = await _jwtService.GenerateTokensAsync(account);

            return new AuthResult(tokens.AccessToken, tokens.RefreshToken)
            {
                UserInfo = new UserInfo
                {
                    DisplayName = command.FullName ?? command.Email,
                    AvatarUrl = string.Empty,
                    DisplayRole = DefaultRoles.Customer
                }
            };
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register-customer", async (Command command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Created($"/users/{result.UserInfo?.DisplayName}", result);
            })
            .WithName("RegisterCustomer")
            .WithTags("Authentication");
        }
    }
}
