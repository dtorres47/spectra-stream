using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpectraStream.Api.Clients;
using SpectraStream.Api.Hubs;
using SpectraStream.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Domain services

// Register typed HttpClient for Streamlabs
builder.Services.AddHttpClient<IStreamlabsClient, StreamlabsClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:3000/mock/streamlabs/");
});

// Register service layer
builder.Services.AddScoped<DonationService>();
builder.Services.AddHttpClient<DonationService>();
builder.Services.AddSingleton<CatalogService>();
builder.Services.AddSingleton<StateService>();
builder.Services.AddSingleton<RequestService>();
builder.Services.AddSingleton<TTSService>();
builder.Services.AddSingleton<QuestService>();

// Swagger/OpenAPI (optional, dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

// SignalR hub
app.MapHub<OverlayHub>("/ws");

// Default route -> overlay
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "index.html")
    );
});

app.MapGet("/overlay", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "index.html")
    );
});

app.MapGet("/admin", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "admin.html"));
});

app.MapGet("/quests", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "quests", "index.html")
    );
});

// Test page
app.MapGet("/test", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "test", "index.html")
    );
});

// Health checks
app.MapGet("/api/health", () =>
    Results.Json(new { status = "ok", service = "spectra-stream" })
);

app.MapGet("/healthz", () =>
    Results.Json(new { status = "ok", service = "spectra-stream" })
);

app.Run("http://localhost:3000");