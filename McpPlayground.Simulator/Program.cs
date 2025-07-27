using McpPlayground.Simulator;
using McpPlayground.Simulator.Configuration;
using McpPlayground.Simulator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();
builder.AddServiceDefaults();

// Configure services
builder.Services.Configure<DeviceConfiguration>(config =>
{
    config.DeviceId = "SIM-001";
    config.DeviceName = "Climate Controller Simulator";
    config.MqttBrokerHost = "localhost";
    config.MqttBrokerPort = 34629;
    config.PublishInterval = TimeSpan.FromSeconds(5);
    config.CommandTopic = "devices/SIM-001/commands";
    config.TelemetryTopic = "devices/SIM-001/telemetry";
    config.StatusTopic = "devices/SIM-001/status";
});

builder.Services.Configure<SimulationConfiguration>(config =>
{
    config.TemperatureSetpoint = 22.0;
    config.HumiditySetpoint = 50.0;
    config.TemperatureMaxThreshold = 26.0;
    config.HumidityMaxThreshold = 65.0;
    config.TemperatureMinThreshold = 18.0;
    config.HumidityMinThreshold = 30.0;
    config.IncreaseChance = 0.7;
    config.MaxTemperatureChange = 0.5;
    config.MaxHumidityChange = 2.0;
});

builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddSingleton<ISimulationEngine, SimulationEngine>();
builder.Services.AddSingleton<DeviceSimulator>();
builder.Services.AddHostedService<DeviceSimulatorService>();

builder.Logging.AddConsole();

Console.WriteLine("Starting McpPlayground.Simulator...");

var host = builder.Build();
await host.RunAsync();
