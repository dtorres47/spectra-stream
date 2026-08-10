using SpectraStream.Api.Configuration;
using SpectraStream.Api.Hubs;
using SpectraStream.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Bind Kofi config (real token supplied via env var Kofi__VerificationToken)
builder.Services.Configure<KofiOptions>(
builder.Configuration.GetSection(KofiOptions.SectionName));

// Quest-catalog services
builder.Services.AddSingleton<IQuestCatalogService, QuestCatalogService>();
builder.Services.AddSingleton<IQuestQueueService, QuestQueueService>();
builder.Services.AddSingleton<SeenMessageTracker>();


// Swagger/OpenAPI (dev only)
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
app.Services.GetRequiredService<IQuestCatalogService>();

// SignalR hub
app.MapHub<OverlayHub>("/ws");

// Page routes
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.MapGet("/overlay", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.MapGet("/store", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "store.html"));
});

app.MapGet("/admin", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "admin.html"));
});

// Health check (kept for deployment — AWS/load balancers ping this)
app.MapGet("/healthz", () =>
    Results.Json(new { status = "ok", service = "spectra-stream" }));

app.Run("http://localhost:3000");