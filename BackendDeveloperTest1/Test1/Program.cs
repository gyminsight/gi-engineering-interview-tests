using Test1.Contracts;
using Test1.Core;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(); 

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddTransient<ISessionFactory, SqliteSessionFactory>();
Dapper.SqlMapper.RemoveTypeMap(typeof(Guid));
Dapper.SqlMapper.AddTypeHandler(MySqlGuidTypeHandler.Default);

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers(); 

app.Run();


// location should be attached to accounts: if an account can be used at multiple locations, then accounts should not be attached to location