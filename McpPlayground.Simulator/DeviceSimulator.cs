using System.Text.Json;
using McpPlayground.Simulator.Configuration;
using McpPlayground.Simulator.Models;
using McpPlayground.Simulator.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace McpPlayground.Simulator;

public class DeviceSimulator
{
    private readonly IMqttService _mqttService;
    private readonly ISimulationEngine _simulationEngine;
    private readonly DeviceConfiguration _config;
    private readonly ILogger<DeviceSimulator> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DeviceSimulator(
        IMqttService mqttService,
        ISimulationEngine simulationEngine,
        IOptions<DeviceConfiguration> options,
        ILogger<DeviceSimulator> logger)
    {
        _mqttService = mqttService;
        _simulationEngine = simulationEngine;
        _config = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttService.ConnectAsync();
        
        // Subscribe to command topic
        await _mqttService.SubscribeAsync(_config.CommandTopic, async payload =>
        {
            try
            {
                var command = JsonSerializer.Deserialize<DeviceCommand>(payload, _jsonOptions);
                if (command != null)
                {
                    _simulationEngine.ProcessCommand(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process command");
            }
        });

        // Publish initial status
        await PublishStatusAsync();
    }

    public async Task StopAsync()
    {
        await PublishStatusAsync("offline");
        await _mqttService.DisconnectAsync();
    }

    public async Task RunSimulationLoopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting simulation loop for device {DeviceId} ({DeviceName})", 
            _config.DeviceId, _config.DeviceName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _simulationEngine.UpdateState();
                await PublishTelemetryAsync();
                
                await Task.Delay(_config.PublishInterval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in simulation loop");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        _logger.LogInformation("Simulation loop ended");
    }

    private async Task PublishTelemetryAsync()
    {
        var state = _simulationEngine.CurrentState;
        var telemetry = new DeviceTelemetry
        {
            DeviceId = _config.DeviceId,
            DeviceName = _config.DeviceName,
            Timestamp = DateTime.UtcNow,
            Sensors = new SensorData
            {
                Temperature = Math.Round(state.Temperature, 2),
                Humidity = Math.Round(state.Humidity, 2),
                PowerConsumption = Math.Round(state.PowerConsumption, 2)
            },
            Actuators = new ActuatorStatus
            {
                Compressor = state.CompressorActive,
                Dehumidifier = state.DehumidifierActive
            },
            OperatingMode = state.Mode.ToString()
        };

        var json = JsonSerializer.Serialize(telemetry, _jsonOptions);
        await _mqttService.PublishAsync(_config.TelemetryTopic, json);
        
        _logger.LogDebug("Published telemetry - Temp: {Temp:F1}°C, Humidity: {Humidity:F1}%, Power: {Power:F0}W", 
            telemetry.Sensors.Temperature, telemetry.Sensors.Humidity, telemetry.Sensors.PowerConsumption);
    }

    private async Task PublishStatusAsync(string status = "online")
    {
        var statusMessage = new
        {
            deviceId = _config.DeviceId,
            deviceName = _config.DeviceName,
            status = status,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(statusMessage, _jsonOptions);
        await _mqttService.PublishAsync(_config.StatusTopic, json);
    }
}