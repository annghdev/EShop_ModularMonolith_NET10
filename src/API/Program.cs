using API;
using API.Middlewares;
using Auth;
using Auth.Data;
using Catalog;
using Catalog.Infrastructure;
using Inventory.Infrastructure;
using Pricing.Infrastructure;
using Users.Infrastructure;
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
builder.Services.AddAuthModule(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapEndpoints(typeof(Catalog.DependencyInjection).Assembly);
app.MapEndpoints(typeof(Auth.DependencyInjection).Assembly);

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

    // Migrate and seed Auth
    var authContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await authContext.Database.MigrateAsync();

    var authSeeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();
    await authSeeder.SeedAsync();

    Console.WriteLine("Auth database migrations applied and data seeded successfully.");

    // Migrate Inventory
    var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await inventoryContext.Database.MigrateAsync();

    Console.WriteLine("Inventory database migrations applied successfully.");

    // Migrate Pricing
    var pricingContext = scope.ServiceProvider.GetRequiredService<PricingDbContext>();
    await pricingContext.Database.MigrateAsync();

    var pricingSeeder = scope.ServiceProvider.GetRequiredService<PricingSeeder>();
    await pricingSeeder.SeedAsync();

    Console.WriteLine("Pricing database migrations applied and data seeded successfully.");

    // Migrate Users
    var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await usersContext.Database.MigrateAsync();

    var usersSeeder = scope.ServiceProvider.GetRequiredService<UsersSeeder>();
    await usersSeeder.SeedAsync();

    Console.WriteLine("Users database migrations applied and data seeded successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database migration or seeding: {ex.Message}");
    throw;
}

app.Run();
