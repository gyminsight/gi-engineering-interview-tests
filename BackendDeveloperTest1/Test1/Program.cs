using Test1.Contracts;
using Test1.Core;
using Serilog;
using Test1.Interfaces;
using Test1.Services;
using Test1.Repositories;
using Test1.Models;

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

//Registering my own interfaces-classes
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRepository<Account>, AccountRepository>();
builder.Services.AddScoped<IRepository<Member>, MemberRepository>();
builder.Services.AddScoped<IGetMembersByAccountService, GetMembersByAccountService>();
builder.Services.AddScoped<IMemberService, MemberService>();





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
