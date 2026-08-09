using CommonLogger;
using Message.Publish.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Transaction_Sql_Crud_Operation.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure master logging toggle and ignored API URLs for telemetry logging
AILogger.SetLoggingEnabled(builder.Configuration.GetValue<bool>("ApplicationInsights:EnableLogging", true));
AILogger.IgnoreApiUrl("/health", "/swagger", "/favicon.ico");

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = false; // Enable automatic model validation
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new GlobalRoutePrefixConvention("api/v1/[controller]"));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

// Register Application Insights Telemetry & CommonLogger
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddCommonLogger();

// Register Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//builder.Services
//    .AddApiVersioning(options =>
//    {
//        options.AssumeDefaultVersionWhenUnspecified = true;
//        options.DefaultApiVersion = new ApiVersion(1, 0);
//        options.ReportApiVersions = true;
//        options.ApiVersionReader = new UrlSegmentApiVersionReader();
//    });


builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
});

// Register the configuration for SwaggerGenOptions to generate documentation for each API version
builder.Services.AddSwaggerGen(s =>
{
    // Define a Swagger document with metadata for the API
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Transaction Sql Crud Operation API", Version = "v1", Contact = new OpenApiContact { Name = "Transaction Developer" } });
});

// ===== SQL Connection & Repository Registration =====
builder.Services.AddTransactionSqlConnection();
builder.Services.AddRepositories(); // Auto-discovers all *Repository classes
// =====================================================

// ===== Azure Service Bus Publisher Registration =====
builder.Services.AddMessagePublisher(builder.Configuration);
// =====================================================

// Add output caching services and define a cache policy for the OpenAPI endpoint 
// to Cache any api add [OutputCache] attribute on the controller or action method, or CacheOutput in minimal api style as shown below.
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("OpenApiCache", policy =>
        policy.Expire(TimeSpan.FromMinutes(10)));
});

builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);

var app = builder.Build();

app.UseExceptionHandler();

// ==========================================
// Add CORS Policy

app.UseCors(builder => builder
  .AllowAnyHeader()
  .AllowAnyMethod()
  .AllowAnyOrigin()
);

// ==========================================


app.UseOutputCache();

// Redirect root URL to health check endpoint for easy monitoring
// This is minimal api style, you can also use app.UseEndpoints() if you prefer the traditional approach
app.MapGet("/", () => "Api is Running");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
       .CacheOutput("OpenApiCache");
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transaction Sql Crud Operation API V1");
    });
}
app.UseAuthorization();
app.MapControllers();


app.Run();