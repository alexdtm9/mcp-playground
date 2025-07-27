using McpPlayground.Simulator;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.AddServiceDefaults();

Console.WriteLine("Starting McpPlayground.Simulator...");

var host = builder.Build();
var device = new Device();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var hostTask = host.RunAsync(cts.Token);

await device.ConnectAsync();
var simulationTask = device.RunSimulationLoopAsync(cts.Token);

try
{
    await Task.WhenAny(hostTask, simulationTask);
}
finally
{
    await device.DisconnectAsync();
}

Console.WriteLine("McpPlayground.Simulator stopped.");