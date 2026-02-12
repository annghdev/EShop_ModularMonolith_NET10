using API;
using API.Middlewares;
using Auth;
using Auth.Data;
using Catalog.Infrastructure;
using Inventory.Infrastructure;
using Pricing.Infrastructure;
using Users.Infrastructure;
using ShoppingCart.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure;
using Payment.Infrastructure;
using Shipping.Infrastructure;
using Wolverine;
using Wolverine.RabbitMQ;
using Wolverine.Postgresql;
using JasperFx;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddElasticsearchClient(connectionName: "elasticsearch");
// Add services to the container.

builder.Services.AddControllers();
// Configure CORS for browser clients that need credentials (cookies)
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    allowedOrigins = ["http://localhost:3000", "https://localhost:3000", "http://localhost:5173", "https://localhost:5173"];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddServices(builder.Configuration);
builder.Services.AddAuthModule(builder.Configuration);

// Configure Wolverine messaging with RabbitMQ
builder.Host.UseWolverine(opts =>
{
    // RabbitMQ Transport
    opts.UseRabbitMq(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!))
        .AutoProvision();
    // ...
    // Transactional Outbox with PostgreSQL
    opts.PersistMessagesWithPostgresql(builder.Configuration.GetConnectionString("infrasdb")!, "public");
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    opts.AutoBuildMessageStorageOnStartup = AutoCreate.CreateOrUpdate;

    // Auto-discover handlers from module assemblies
    opts.Discovery.IncludeAssembly(typeof(Catalog.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Inventory.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Pricing.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Orders.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Payment.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(ShoppingCart.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Shipping.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Users.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Auth.DependencyInjection).Assembly);
});


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
app.UseCors("Frontend");

app.MapGet("/", () =>
{
    return Results.Ok("Server is running...");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapEndpoints(typeof(Catalog.DependencyInjection).Assembly);
app.MapEndpoints(typeof(Auth.DependencyInjection).Assembly);
app.MapEndpoints(typeof(Inventory.DependencyInjection).Assembly);
app.MapEndpoints(typeof(ShoppingCart.DependencyInjection).Assembly);
app.MapEndpoints(typeof(API.DependencyInjection).Assembly);

// Apply migrations and seed data
using var scope = app.Services.CreateScope();
try
{
    // Migrate Catalog
    var catalogContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await catalogContext.Database.MigrateAsync();

    var catalogSeeder = scope.ServiceProvider.GetRequiredService<CatalogSeeder>();
    await catalogSeeder.SeedAsync();

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

    var inventorySeeder = scope.ServiceProvider.GetRequiredService<InventorySeeder>();
    await inventorySeeder.SeedAsync();

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

    // Migrate ShoppingCart
    var shoppingCartContext = scope.ServiceProvider.GetRequiredService<ShoppingCartDbContext>();
    await shoppingCartContext.Database.MigrateAsync();

    var shoppingCartSeeder = scope.ServiceProvider.GetRequiredService<ShoppingCartSeeder>();
    await shoppingCartSeeder.SeedAsync();

    Console.WriteLine("ShoppingCart database migrations applied and data seeded successfully.");

    // Migrate Orders
    var ordersContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await ordersContext.Database.MigrateAsync();

    var ordersSeeder = scope.ServiceProvider.GetRequiredService<OrdersSeeder>();
    await ordersSeeder.SeedAsync();

    Console.WriteLine("Orders database migrations applied and data seeded successfully.");

    // Migrate Payment
    var paymentContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await paymentContext.Database.MigrateAsync();

    var paymentSeeder = scope.ServiceProvider.GetRequiredService<PaymentSeeder>();
    await paymentSeeder.SeedAsync();

    Console.WriteLine("Payment database migrations applied and data seeded successfully.");

    // Migrate Shipping
    var shippingContext = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
    await shippingContext.Database.MigrateAsync();

    var shippingSeeder = scope.ServiceProvider.GetRequiredService<ShippingSeeder>();
    await shippingSeeder.SeedAsync();

    Console.WriteLine("Shipping database migrations applied and data seeded successfully.");

    // Migrate Shipping
    var infrasConext = scope.ServiceProvider.GetRequiredService<InfrasDbContext>();
    await infrasConext.Database.MigrateAsync();

    Console.WriteLine("Infras database migrations applied and data seeded successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database migration or seeding: {ex.Message}");
    throw;
}

app.Run();
