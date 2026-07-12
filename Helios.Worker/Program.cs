using Helios.Application.Extensions;
using Helios.Infrastructure.Extensions;
using Helios.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Register application and infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Register messaging with consumers from the current assembly
builder.Services.AddMessaging(builder.Configuration, typeof(Program).Assembly);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

