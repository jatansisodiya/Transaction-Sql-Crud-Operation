using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Transaction_Sql_Crud_Operation.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new GlobalRoutePrefixConvention("api/v1/[controller]"));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddExceptionHandler<>(); need to know the ussage 
builder.Services.AddProblemDetails();

//builder.Services
//    .AddApiVersioning(options =>
//    {
//        options.AssumeDefaultVersionWhenUnspecified = true;
//        options.DefaultApiVersion = new ApiVersion(1, 0);
//        options.ReportApiVersions = true;
//        options.ApiVersionReader = new UrlSegmentApiVersionReader();
//    });


builder.Services.AddOpenApi();

// Register the configuration for SwaggerGenOptions to generate documentation for each API version
builder.Services.AddSwaggerGen(s =>
{
    // Define a Swagger document with metadata for the API
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Transaction Sql Crud Operation API", Version = "v1", Contact = new OpenApiContact { Name = "Transaction Developer" } });

    // Register the custom operation filter for swagger
    //s.OperationFilter<SwaggerHeadersFilter>();
});

var app = builder.Build();

// Redirect root URL to health check endpoint for easy monitoring
// This is mininmal api style, you can also use app.UseEndpoints() if you prefer the traditional approach
app.MapGet("/", () => "Api is Running");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transaction Sql Crud Operation API V1");
    });
}
app.UseAuthorization();
app.MapControllers();


app.Run();