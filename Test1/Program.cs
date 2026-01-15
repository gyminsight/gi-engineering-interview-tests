using Test1.Contracts;
using Test1.Core;
using Test1.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Customize validation error responses
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var problemDetails = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Instance = context.HttpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = context.HttpContext.TraceIdentifier
                }
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problemDetails);
        };
    });

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks();

// Database services
builder.Services.AddTransient<ISessionFactory, SqliteSessionFactory>();

// Configure Dapper type handlers
Dapper.SqlMapper.RemoveTypeMap(typeof(Guid));
Dapper.SqlMapper.AddTypeHandler(MySqlGuidTypeHandler.Default);

var app = builder.Build();

// Configure the HTTP request pipeline

// Global exception handling (should be first in pipeline)
app.UseGlobalExceptionHandling();

// Request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// Log startup information
Log.Information("Starting Gym Management API on {Environment}", app.Environment.EnvironmentName);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
