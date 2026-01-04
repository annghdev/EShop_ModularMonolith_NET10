using Catalog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Catalog.API;

public class SeedEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/catalog/seed", async ([FromServices] CatalogSeeder seeder) =>
        {
            try
            {
                await seeder.SeedAsync();
                return Results.Ok(new { message = "Catalog data seeded successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to seed catalog data: {ex.Message}");
            }
        })
        .WithTags("Catalog")
        .WithName("SeedCatalogData")
        .Produces(200)
        .ProducesProblem(500);
    }
}
