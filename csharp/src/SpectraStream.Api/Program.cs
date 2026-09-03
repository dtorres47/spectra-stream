using SpectraStream.Api.Configuration;
using SpectraStream.Api.Hubs;
using SpectraStream.Api.Middleware;
using SpectraStream.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddDataProtection();

// Bind Kofi config (real token supplied via env var Kofi__VerificationToken)
builder.Services.Configure<KofiOptions>(
builder.Configuration.GetSection(KofiOptions.SectionName));

// Bind Admin config (real key supplied via env var Admin__SharedKey)
builder.Services.Configure<AdminOptions>(
builder.Configuration.GetSection(AdminOptions.SectionName));

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

app.UseMiddleware<AdminAuthMiddleware>();
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

app.MapGet("/admin", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "admin.html"));
});

app.MapGet("/overlay", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.MapGet("/login", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "login.html"));
});

app.MapGet("/store", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "store.html"));
});





// Health check (kept for deployment — AWS/load balancers ping this)
app.MapGet("/healthz", () =>
    Results.Json(new { status = "ok", service = "spectra-stream" }));

app.Run();