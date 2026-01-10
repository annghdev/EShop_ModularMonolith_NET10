using API;
using API.Middlewares;
using Catalog;
using Catalog.Infrastructure;
using Inventory.Infrastructure;
using Kernel.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddElasticsearchClient(connectionName: "elasticsearch");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return Results.Ok("Server is running...");
});

app.UseAuthorization();

app.MapControllers();

app.MapEndpoints(typeof(Catalog.DependencyInjection).Assembly);

// Apply migrations and seed data
using var scope = app.Services.CreateScope();
try
{
    // Migrate Catalog
    var catalogContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await catalogContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<CatalogSeeder>();
    await seeder.SeedAsync();

    Console.WriteLine("Catalog database migrations applied and data seeded successfully.");


    // Migrate Inventory
    var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await inventoryContext.Database.MigrateAsync();

    Console.WriteLine("Catalog database migrations applied and data seeded successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database migration or seeding: {ex.Message}");
    throw;
}

app.Run();
