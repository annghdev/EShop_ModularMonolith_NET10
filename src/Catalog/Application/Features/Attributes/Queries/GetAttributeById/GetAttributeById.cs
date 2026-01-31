using Contracts.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class GetAttributeById
{
    public record Query(Guid Id) : IRequest<AttributeDto>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Id)
                .NotEmpty().WithMessage("Attribute ID cannot be empty");
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, AttributeDto>
    {
        public async Task<AttributeDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var attribute = await uow.Attributes
                .Where(a => !a.IsDeleted && a.Id == query.Id)
                .Include(a => a.Values)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (attribute == null)
            {
                throw new NotFoundException("Attribute", query.Id);
            }

            return new AttributeDto
            {
                Id = attribute.Id,
                Name = attribute.Name,
                Icon = attribute.Icon,
                ValueStyleCss = attribute.ValueStyleCss,
                DisplayText = attribute.DisplayText,
                Values = attribute.Values.Select(v => new AttributeValueDto
                {
                    Id = v.Id,
                    Value = v.Name,
                    ColorCode = v.ColorCode
                }).ToList()
            };
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/attributes/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new Query(id));
                return Results.Ok(result);
            })
            .WithTags("Attributes")
            .WithName("GetAttributeById");
        }
    }
}
