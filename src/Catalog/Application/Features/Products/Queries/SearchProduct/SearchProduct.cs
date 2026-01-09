using Elastic.Clients.Elasticsearch;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class SearchProduct
{
    public record Query(
        string? Keyword = null,
        string? CategoryId = null,
        string? BrandId = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        string? AttributeFilters = null, // JSON string: {"color":"red","size":"M"}
        int Page = 1,
        int PageSize = 20,
        string? SortBy = "createdAt",
        string? SortOrder = "asc"
    ) : IRequest<PaginatedResult<ProductProjection>>;

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

        private bool BeValidSortField(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy) || sortBy == "createdAt") return true; // Allow default value

            var validFields = new[] { "name", "price", "relevance" };
            return validFields.Contains(sortBy.ToLower());
        }
    }

    public class Handler(ElasticsearchClient elasticsearchClient) : IRequestHandler<Query, PaginatedResult<ProductProjection>>
    {
        private const string IndexName = "products";

        public async Task<PaginatedResult<ProductProjection>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Set default values
            var sortBy = query.SortBy ?? "createdAt";
            var sortOrder = query.SortOrder ?? "asc";

            var response = await elasticsearchClient.SearchAsync<ProductProjection>(s => s
                .Indices(IndexName)
                .From((query.Page - 1) * query.PageSize)
                .Size(query.PageSize)
                .Query(q => q
                    .Bool(b =>
                    {
                        // MUST: keyword search
                        if (!string.IsNullOrEmpty(query.Keyword))
                        {
                            b.Must(m => m
                                .Match(mm => mm
                                    .Field("name")
                                    .Query(query.Keyword)
                                    .Fuzziness("AUTO")
                                )
                            );
                        }
                        else
                        {
                            // If no keyword, match all
                            b.Must(m => m.MatchAll());
                        }

                        // FILTER: category
                        if (!string.IsNullOrEmpty(query.CategoryId))
                        {
                            b.Filter(f => f
                                .Term(t => t.Field("categoryId").Value(query.CategoryId))
                            );
                        }

                        // FILTER: brand
                        if (!string.IsNullOrEmpty(query.BrandId))
                        {
                            b.Filter(f => f
                                .Term(t => t.Field("brandId").Value(query.BrandId))
                            );
                        }

                        // FILTER: price range
                        if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
                        {
                            b.Filter(f => f
                                .Range(r => r
                                    .Number(nr => nr
                                        .Field("price")
                                        .Gte(query.MinPrice.HasValue ? (double)query.MinPrice.Value : null)
                                        .Lte(query.MaxPrice.HasValue ? (double)query.MaxPrice.Value : null)
                                    )
                                )
                            );
                        }

                        // FILTER: attributes
                        if (!string.IsNullOrEmpty(query.AttributeFilters))
                        {
                            try
                            {
                                var attributeDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(query.AttributeFilters);
                                if (attributeDict != null && attributeDict.Count > 0)
                                {
                                    foreach (var attr in attributeDict)
                                    {
                                        b.Filter(f => f
                                            .Nested(n => n
                                                .Path("variants.attributes")
                                                .Query(nq => nq
                                                    .Bool(nb => nb
                                                        .Must(
                                                            m1 => m1.Term(t => t.Field("variants.attributes.attributeName").Value(attr.Key)),
                                                            m2 => m2.Term(t => t.Field("variants.attributes.valueName").Value(attr.Value))
                                                        )
                                                    )
                                                )
                                            )
                                        );
                                    }
                                }
                            }
                            catch
                            {
                                // Invalid JSON, ignore attribute filters
                            }
                        }
                    })
                ),
                cancellationToken
            );

            if (!response.IsSuccess())
            {
                throw new InvalidOperationException($"Search failed: {response.DebugInformation}");
            }

            var items = response.Documents.ToList();

            return new PaginatedResult<ProductProjection>(query.Page, query.PageSize, items,(int)response.Total);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products/search", async (
                string? keyword = null,
                string? categoryId = null,
                string? brandId = null,
                decimal? minPrice = null,
                decimal? maxPrice = null,
                string? attributeFilters = null,
                int page = 1,
                int pageSize = 20,
                string? sortBy = "createdAt",
                string? sortOrder = "asc",
                ISender sender = null!) =>
            {
                var query = new Query(
                    keyword, categoryId, brandId, minPrice, maxPrice,
                    attributeFilters, page, pageSize, sortBy, sortOrder);

                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithTags("Products")
            .WithName("SearchProducts");

            // Debug endpoint to get all items
            app.MapGet("api/products/search/all", async (ElasticsearchClient elasticsearchClient) =>
            {
                try
                {
                    var response = await elasticsearchClient.SearchAsync<ProductProjection>(s => s
                        .Indices("products")
                        .Size(1000) // Get up to 1000 items
                        .Query(q => q.MatchAll())
                    );

                    if (!response.IsSuccess())
                    {
                        return Results.Problem($"Search failed: {response.DebugInformation}", statusCode: 500);
                    }

                    var items = response.Documents.Select(d => new
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        Price = d.Price,
                        Currency = d.Currency,
                        CategoryName = d.CategoryName,
                        BrandName = d.BrandName,
                        CreatedAt = d.CreatedAt
                    }).ToList();

                    return Results.Ok(new
                    {
                        Total = response.Total,
                        Items = items
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error: {ex.Message}", statusCode: 500);
                }
            })
            .WithTags("Products")
            .WithName("GetAllProducts");
        }
    }
}