using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var simulator = builder.AddProject<McpPlayground_Simulator>("simulator");

builder.Build().Run();