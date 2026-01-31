using Catalog.Domain;
using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class UpdateCategory
{
    public record Command(Guid Id, UpdateCategoryRequest Request) : IRequest
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["categories_all", "categories_tree", $"category_detail_{Id}", $"category_attributes_{Id}"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Category ID cannot be empty");

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

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var category = await uow.Categories.AsQueryable()
                .Include(c => c.DefaultAttributes)
                .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException("Category", command.Id);
            }

            if (command.Request.ParentId.HasValue)
            {
                if (command.Request.ParentId.Value == command.Id)
                {
                    throw new ConflictException("Update category parent");
                }

                var parentExists = await uow.Categories.AsQueryable()
                    .AnyAsync(c => c.Id == command.Request.ParentId.Value, cancellationToken);

                if (!parentExists)
                {
                    throw new NotFoundException("Category", command.Request.ParentId.Value);
                }

                await EnsureNoCircularParent(command.Id, command.Request.ParentId.Value, cancellationToken);
            }

            category.Name = command.Request.Name;
            category.Image = command.Request.Image;
            category.ParentId = command.Request.ParentId;

            var defaultAttributes = command.Request.DefaultAttributes
                .Select(attr => new CategoryDefaultAttribute
                {
                    AttributeId = attr.AttributeId,
                    CategoryId = category.Id,
                    DisplayOrder = attr.DisplayOrder
                })
                .ToList();

            category.SetDefaultAttributes(defaultAttributes);
            uow.Categories.Update(category);
            await uow.CommitAsync(cancellationToken);
        }

        private async Task EnsureNoCircularParent(Guid categoryId, Guid parentId, CancellationToken cancellationToken)
        {
            var currentId = parentId;
            while (true)
            {
                var parentInfo = await uow.Categories.AsQueryable()
                    .Where(c => c.Id == currentId)
                    .Select(c => c.ParentId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!parentInfo.HasValue)
                {
                    return;
                }

                if (parentInfo.Value == categoryId)
                {
                    throw new ConflictException("Update category parent");
                }

                currentId = parentInfo.Value;
            }
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/categories/{id:guid}", async (Guid id, UpdateCategoryRequest request, ISender sender) =>
            {
                await sender.Send(new Command(id, request));
                return Results.NoContent();
            })
            .WithTags("Categories")
            .WithName("UpdateCategory")
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
