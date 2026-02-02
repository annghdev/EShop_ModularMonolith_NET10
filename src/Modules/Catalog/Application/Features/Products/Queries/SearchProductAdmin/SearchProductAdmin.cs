using Catalog.Domain;
using Contracts.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// EF Core-based product search with pagination and filtering for admin site.
/// Unlike SearchProduct (Elasticsearch), this uses direct database queries.
/// </summary>
public class SearchProductAdmin
{
    public record Query(
        string? Keyword = null,
        Guid? CategoryId = null,
        Guid? BrandId = null,
        ProductStatus? Status = null,
        int Page = 1,
        int PageSize = 20,
        string? SortBy = "createdAt",
        string? SortOrder = "desc"
    ) : IRequest<PaginatedResult<ProductSearchDto>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(q => q.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(q => q.SortBy)
                .Must(BeValidSortField).WithMessage("Invalid sort field");

            RuleFor(q => q.SortOrder)
                .Must(so => string.IsNullOrEmpty(so) || so == "asc" || so == "desc")
                .WithMessage("SortOrder must be 'asc' or 'desc'");
        }

        private static bool BeValidSortField(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy)) return true;
            var validFields = new[] { "createdat", "name", "price", "status" };
            return validFields.Contains(sortBy.ToLower());
        }
    }

    public class Handler(ICatalogUnitOfWork uow, IMapper mapper) : IRequestHandler<Query, PaginatedResult<ProductSearchDto>>
    {
        public async Task<PaginatedResult<ProductSearchDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var dbQuery = uow.Products.AsQueryable()
                .Where(p => p.Status != ProductStatus.Draft)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .AsNoTracking();

            // Keyword filter (search in name and description)
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.ToLower();
                dbQuery = dbQuery.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)));
            }

            // Category filter
            if (query.CategoryId.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            // Brand filter
            if (query.BrandId.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.BrandId == query.BrandId.Value);
            }

            // Status filter
            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Status == query.Status.Value);
            }

            // Get total count before pagination
            var total = await dbQuery.CountAsync(cancellationToken);

            // Sorting
            var sortBy = query.SortBy?.ToLower() ?? "createdat";
            var isDescending = query.SortOrder?.ToLower() == "desc";

            dbQuery = sortBy switch
            {
                "name" => isDescending ? dbQuery.OrderByDescending(p => p.Name) : dbQuery.OrderBy(p => p.Name),
                "price" => isDescending ? dbQuery.OrderByDescending(p => p.Price.Amount) : dbQuery.OrderBy(p => p.Price.Amount),
                "status" => isDescending ? dbQuery.OrderByDescending(p => p.Status) : dbQuery.OrderBy(p => p.Status),
                _ => isDescending ? dbQuery.OrderByDescending(p => p.CreatedAt) : dbQuery.OrderBy(p => p.CreatedAt)
            };

            // Pagination
            var items = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var dtos = items.Select(p => new ProductSearchDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Slug = p.Slug.Value,
                Sku = p.Variants.FirstOrDefault()?.Sku.Value ?? string.Empty,
                Price = new MoneyDto(p.Price.Amount, p.Price.Currency),
                CategoryName = p.Category?.Name ?? string.Empty,
                BrandName = p.Brand?.Name ?? string.Empty,
                Thumbnail = p.Thumbnail?.Path,
                Status = p.Status.ToString(),
                VariantCount = p.Variants.Count,
                CreatedAt = p.CreatedAt.DateTime
            }).ToList();

            return new PaginatedResult<ProductSearchDto>(query.Page, query.PageSize, dtos, total);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/products", async (
                string? keyword,
                Guid? categoryId,
                Guid? brandId,
                string? status,
                int page = 1,
                int pageSize = 20,
                string? sortBy = "createdAt",
                string? sortOrder = "desc",
                ISender sender = null!) =>
            {
                ProductStatus? statusEnum = null;
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<ProductStatus>(status, true, out var parsed))
                {
                    statusEnum = parsed;
                }

                var query = new Query(keyword, categoryId, brandId, statusEnum, page, pageSize, sortBy, sortOrder);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithTags("Admin Products")
            .WithName("SearchProductsAdmin")
            .RequireAuthorization();
        }
    }
}
