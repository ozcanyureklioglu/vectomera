var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Helios_Api>("api");

var worker = builder.AddProject<Projects.Helios_Worker>("worker");

builder.Build().Run();
