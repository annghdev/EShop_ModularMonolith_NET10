using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class DeleteCategory
{
    public record Command(Guid Id) : IRequest
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["categories_all", "categories_tree", $"category_detail_{Id}", $"category_attributes_{Id}"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Category ID cannot be empty");
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var category = await uow.Categories.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException("Category", command.Id);
            }

            var hasChildren = await uow.Categories.AsQueryable()
                .AnyAsync(c => c.ParentId == command.Id, cancellationToken);

            if (hasChildren)
            {
                throw new ConflictException("Delete category");
            }

            uow.Categories.Remove(category);
            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/categories/{id:guid}", async (Guid id, ISender sender) =>
            {
                await sender.Send(new Command(id));
                return Results.NoContent();
            })
            .WithTags("Categories")
            .WithName("DeleteCategory")
            .RequireAuthorization();
        }
    }
}
