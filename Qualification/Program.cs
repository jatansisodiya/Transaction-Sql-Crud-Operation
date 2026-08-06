using CommonLogger;

var builder = WebApplication.CreateBuilder(args);

// Configure master logging toggle and ignored API URLs for telemetry logging
AILogger.SetLoggingEnabled(builder.Configuration.GetValue<bool>("ApplicationInsights:EnableLogging", true));
AILogger.IgnoreApiUrl("/health", "/favicon.ico");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .AddRazorPages()
    .AddRazorRuntimeCompilation();

// Register Application Insights & CommonLogger
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddCommonLogger();

// Register Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);

var app = builder.Build();

app.UseExceptionHandler();

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
