using System.Text;
using System.Text.Json;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using McpPlayground.Shared.Data;
using McpPlayground.Shared.Data.Models;
using McpPlayground.TelemetryCollector.Configuration;
using McpPlayground.TelemetryCollector.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace McpPlayground.TelemetryCollector.Services;

public class TelemetryCollectorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelemetryCollectorService> _logger;
    private readonly CollectorConfiguration _config;
    private HiveMQClient? _mqttClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TelemetryCollectorService(
        IServiceProvider serviceProvider,
        IOptions<CollectorConfiguration> config,
        ILogger<TelemetryCollectorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken); // Wait for services to start

        _logger.LogInformation("Starting Telemetry Collector Service");

        try
        {
            await ConnectToMqttAsync();
            await SubscribeToTopicsAsync();

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in telemetry collector service");
            throw;
        }
    }

    private async Task ConnectToMqttAsync()
    {
        var mqttOptions = new HiveMQClientOptionsBuilder()
            .WithBroker(_config.MqttBrokerHost)
            .WithPort(_config.MqttBrokerPort)
            .WithClientId($"telemetry-collector-{Guid.NewGuid():N}")
            .Build();

        _mqttClient = new HiveMQClient(mqttOptions);
        await _mqttClient.ConnectAsync();
        _logger.LogInformation("Connected to MQTT broker at {Host}:{Port}", 
            _config.MqttBrokerHost, _config.MqttBrokerPort);
    }

    private async Task SubscribeToTopicsAsync()
    {
        if (_mqttClient == null) throw new InvalidOperationException("MQTT client not initialized");

        // Subscribe to telemetry
        _mqttClient.OnMessageReceived += async (sender, args) =>
        {
            var topic = args.PublishMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.PublishMessage.Payload ?? Array.Empty<byte>());

            try
            {
                if (topic.EndsWith("/telemetry"))
                {
                    await ProcessTelemetryAsync(payload);
                }
                else if (topic.EndsWith("/commands"))
                {
                    await ProcessCommandAsync(payload);
                }
                else if (topic.EndsWith("/status"))
                {
                    await ProcessStatusAsync(payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
            }
        };

        await _mqttClient.SubscribeAsync(_config.TelemetryTopicPattern, QualityOfService.AtLeastOnceDelivery);
        await _mqttClient.SubscribeAsync(_config.CommandTopicPattern, QualityOfService.AtLeastOnceDelivery);
        await _mqttClient.SubscribeAsync(_config.StatusTopicPattern, QualityOfService.AtLeastOnceDelivery);

        _logger.LogInformation("Subscribed to MQTT topics");
    }

    private async Task ProcessTelemetryAsync(string payload)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<McpDbContext>();

        var telemetry = JsonSerializer.Deserialize<TelemetryDto>(payload, _jsonOptions);
        if (telemetry == null) return;

        var record = new TelemetryRecord
        {
            DeviceId = telemetry.DeviceId,
            DeviceName = telemetry.DeviceName,
            Timestamp = telemetry.Timestamp,
            Temperature = telemetry.Sensors.Temperature,
            Humidity = telemetry.Sensors.Humidity,
            PowerConsumption = telemetry.Sensors.PowerConsumption,
            CompressorActive = telemetry.Actuators.Compressor,
            DehumidifierActive = telemetry.Actuators.Dehumidifier,
            OperatingMode = telemetry.OperatingMode
        };

        dbContext.TelemetryRecords.Add(record);
        await dbContext.SaveChangesAsync();
        
        _logger.LogDebug("Saved telemetry for device {DeviceId}", telemetry.DeviceId);
    }

    private async Task ProcessCommandAsync(string payload)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<McpDbContext>();

        var command = JsonSerializer.Deserialize<CommandDto>(payload, _jsonOptions);
        if (command == null) return;

        var record = new CommandRecord
        {
            DeviceId = ExtractDeviceIdFromTopic(), // You'll need to implement this
            Command = command.Command,
            Parameters = command.Parameters != null ? JsonSerializer.Serialize(command.Parameters) : null,
            Timestamp = command.Timestamp
        };

        dbContext.CommandRecords.Add(record);
        await dbContext.SaveChangesAsync();

        _logger.LogDebug("Saved command {Command}", command.Command);
    }

    private async Task ProcessStatusAsync(string payload)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<McpDbContext>();

        var status = JsonSerializer.Deserialize<StatusDto>(payload, _jsonOptions);
        if (status == null) return;

        var record = new StatusRecord
        {
            DeviceId = status.DeviceId,
            DeviceName = status.DeviceName,
            Status = status.Status,
            Timestamp = status.Timestamp
        };

        dbContext.StatusRecords.Add(record);
        await dbContext.SaveChangesAsync();

        _logger.LogDebug("Saved status for device {DeviceId}: {Status}", status.DeviceId, status.Status);
    }

    private string ExtractDeviceIdFromTopic()
    {
        // For now, return the configured device ID
        // In a real scenario, you'd extract from the MQTT topic
        return "SIM-001";
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Telemetry Collector Service");
        
        if (_mqttClient?.IsConnected() == true)
        {
            await _mqttClient.DisconnectAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}