using Microsoft.ApplicationInsights;
using Microsoft.IdentityModel.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .AddRazorPages()
    .AddRazorRuntimeCompilation();

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();
var telemetryConfig = app.Services.GetRequiredService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>();

Console.WriteLine($"AI Connection String: {telemetryConfig.ConnectionString}");
// Test Application Insights on startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application Insights Startup Test");

var telemetry = app.Services.GetRequiredService<TelemetryClient>();

telemetry.TrackTrace("AI_TEST_DIRECT");
telemetry.Flush();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
