using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

Console.WriteLine("Starting McpPlayground.Simulator...");

var host = builder.Build();

Console.WriteLine("McpPlayground.Simulator started.");

await host.RunAsync();