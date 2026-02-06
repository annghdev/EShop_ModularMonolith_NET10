using BlazorAdmin;
using BlazorAdmin.Auth;
using BlazorAdmin.Components;
using BlazorAdmin.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAntDesign();

// Add controllers for API endpoints (AuthController)
builder.Services.AddControllers();

// Add HttpContext accessor for reading cookies in Blazor
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for internal API calls (Login.razor)
builder.Services.AddScoped(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var request = httpContextAccessor.HttpContext?.Request;
    var baseUri = $"{request?.Scheme}://{request?.Host}";
    return new HttpClient { BaseAddress = new Uri(baseUri) };
});

// Configure Refit client for Auth API (using Aspire service discovery)
builder.Services.AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http+https://eshop-api"));

// Authentication scheme (required for [Authorize] attribute, but NO redirect)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "BlazorServerAuth";
    options.DefaultChallengeScheme = "BlazorServerAuth";
})
.AddCookie("BlazorServerAuth", options =>
{
    // Blazor handles redirect via AuthorizeRouteView, NOT middleware
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

// Authorization services
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
