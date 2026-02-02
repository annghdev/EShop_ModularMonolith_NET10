using Catalog.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class UpdateProductPricing
{
    public record Request
    {
        public Guid Id { get; init; }
        public MoneyDto Cost { get; init; }
        public MoneyDto Price { get; init; }
    }

    public record Command(Request Request) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [$"product_{Request.Id}"];
        public IEnumerable<string> CacheKeyPrefix => ["product"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.Id)
                .NotEmpty().WithMessage("Id cannot be empty");

            RuleFor(c => c.Request.Cost)
                .NotNull().WithMessage("Cost cannot be null")
                .Must(cost => cost.Amount >= 0).WithMessage("Cost amount must be greater than or equal to 0");

            RuleFor(c => c.Request.Price)
                .NotNull().WithMessage("Price cannot be null")
                .Must(price => price.Amount >= 0).WithMessage("Price amount must be greater than or equal to 0")
                .Must((command, price) => price.Amount >= command.Request.Cost.Amount)
                .WithMessage("Price amount must be greater than or equal to cost amount");
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var product = await uow.Products.LoadFullAggregate(request.Id)
                ?? throw new NotFoundException("Product", request.Id);

            product.UpdatePricing(request.Cost.ToMoney(), request.Price.ToMoney());

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/{id}/pricing", async (Guid id, Request request, ISender sender) =>
            {
                request = request with { Id = id };
                await sender.Send(new Command(request));
                return Results.NoContent();
            })
                .WithName("UpdateProductPricing")
                .WithTags("Products");
        }
    }
}
