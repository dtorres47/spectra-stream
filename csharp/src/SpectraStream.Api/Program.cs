using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpectraStream.Api.Hubs;
using SpectraStream.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Domain services
builder.Services.AddSingleton<DonationService>();
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

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

// SignalR hub
app.MapHub<OverlayHub>("/ws");

// Overlay + panel routes
app.MapGet("/overlay", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.MapGet("/panel", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "panel.html"));
});

// Health check
app.MapGet("/api/health", () =>
    Results.Json(new { status = "ok", service = "spectra-stream" })
);

app.Run("http://localhost:3000");
