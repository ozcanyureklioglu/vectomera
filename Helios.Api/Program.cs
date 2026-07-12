using Helios.Api;
using Helios.Api.Extensions;
using Helios.Application.Extensions;
using Helios.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom endpoints from the current assembly
builder.Services.AddEndpoints(typeof(Program).Assembly);

// Add application layer and infrastructure layer services here
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration); // Pass no assemblies for publisher-only setup

var app = builder.Build();

app.UseMiddleware<Helios.Api.Middlewares.ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map all registered endpoints
var apiGroup = app.MapGroup("/api");
app.MapEndpoints(apiGroup);

app.Run();
