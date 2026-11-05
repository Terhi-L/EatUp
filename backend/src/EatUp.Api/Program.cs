using System.Text.Json.Serialization;
using EatUp.Api;
using EatUp.Api.Data;
using EatUp.Api.Domain;
using EatUp.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Local SQLite file and generated VAPID keys live in backend/data (git-ignored).
var dataDir = builder.Configuration["DataDirectory"]
    ?? Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data"));
Directory.CreateDirectory(dataDir);

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<EatUpDb>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("EatUp") ?? $"Data Source={Path.Combine(dataDir, "eatup.db")}"));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(_ => Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json")));
builder.Services.AddSingleton(sp => VapidKeyStore.LoadOrCreate(
    sp.GetRequiredService<IConfiguration>(), dataDir, sp.GetRequiredService<ILoggerFactory>().CreateLogger("Push")));
builder.Services.AddSingleton<IPushSender, WebPushSender>();
builder.Services.AddHttpClient<IProductLookup, OpenFoodFactsLookup>(http =>
{
    http.BaseAddress = OpenFoodFactsLookup.BaseAddress;
    http.Timeout = TimeSpan.FromSeconds(10);
    // Open Food Facts asks every app to identify itself.
    http.DefaultRequestHeaders.UserAgent.ParseAdd("EatUp/0.1 (local food-waste app)");
});
builder.Services.AddScoped<Pantry>();
builder.Services.AddScoped<DailyCheck>();
if (builder.Configuration.GetValue("Push:EnableDailyJob", true))
    builder.Services.AddHostedService<DailyNotificationJob>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EatUpDb>();
    await DatabaseSetup.PrepareAsync(db, scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Database"));
    await DemoData.EnsureSeededAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
}

app.MapEatUpApi();

app.Run();

public partial class Program;
