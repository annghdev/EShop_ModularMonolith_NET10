using Catalog.Domain;
using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class CreateCategory
{
    public record Command(CreateCategoryRequest Request) : IRequest<Guid>
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["categories_all", "categories_tree"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Category name cannot be empty")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");

            RuleFor(c => c.Request.Image)
                .MaximumLength(500).WithMessage("Image cannot exceed 500 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Request.Image));

            RuleForEach(c => c.Request.DefaultAttributes).SetValidator(new CategoryDefaultAttributeRequestValidator());

            RuleFor(c => c.Request.DefaultAttributes)
                .Must(attrs => attrs.Select(a => a.AttributeId).Distinct().Count() == attrs.Count)
                .WithMessage("Default attributes must be unique");
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command command, CancellationToken cancellationToken)
        {
            if (command.Request.ParentId.HasValue)
            {
                var parentExists = await uow.Categories.AsQueryable()
                    .AnyAsync(c => c.Id == command.Request.ParentId.Value, cancellationToken);

                if (!parentExists)
                {
                    throw new NotFoundException("Category", command.Request.ParentId.Value);
                }
            }

            var category = new Category
            {
                Id = Guid.CreateVersion7(),
                Name = command.Request.Name,
                Image = command.Request.Image,
                ParentId = command.Request.ParentId
            };

            var defaultAttributes = command.Request.DefaultAttributes
                .Select(attr => new CategoryDefaultAttribute
                {
                    AttributeId = attr.AttributeId,
                    CategoryId = category.Id,
                    DisplayOrder = attr.DisplayOrder
                })
                .ToList();

            category.SetDefaultAttributes(defaultAttributes);

            uow.Categories.Add(category);
            await uow.CommitAsync(cancellationToken);
            return category.Id;
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/categories", async (CreateCategoryRequest request, ISender sender) =>
            {
                var id = await sender.Send(new Command(request));
                return Results.Created($"api/categories/{id}", new { id });
            })
            .WithTags("Categories")
            .WithName("CreateCategory")
            .RequireAuthorization();
        }
    }

    private sealed class CategoryDefaultAttributeRequestValidator : AbstractValidator<CategoryDefaultAttributeRequest>
    {
        public CategoryDefaultAttributeRequestValidator()
        {
            RuleFor(c => c.AttributeId)
                .NotEmpty().WithMessage("Attribute ID cannot be empty");

            RuleFor(c => c.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be >= 0");
        }
    }
}
