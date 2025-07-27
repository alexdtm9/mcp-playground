using System.Text;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using McpPlayground.Simulator.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace McpPlayground.Simulator.Services;

public interface IMqttService
{
    Task ConnectAsync();
    Task DisconnectAsync();
    Task PublishAsync(string topic, string payload);
    Task SubscribeAsync(string topic, Func<string, Task> messageHandler);
    bool IsConnected { get; }
}

public class MqttService : IMqttService
{
    private readonly HiveMQClient _client;
    private readonly ILogger<MqttService> _logger;
    private readonly DeviceConfiguration _config;

    public bool IsConnected => _client.IsConnected();

    public MqttService(IOptions<DeviceConfiguration> options, ILogger<MqttService> logger)
    {
        _logger = logger;
        _config = options.Value;

        var mqttOptions = new HiveMQClientOptionsBuilder()
            .WithBroker(_config.MqttBrokerHost)
            .WithPort(_config.MqttBrokerPort)
            .WithClientId($"{_config.DeviceId}-{Guid.NewGuid():N}")
            .Build();

        _client = new HiveMQClient(mqttOptions);
    }

    public async Task ConnectAsync()
    {
        try
        {
            await _client.ConnectAsync();
            _logger.LogInformation("Connected to MQTT broker at {Host}:{Port}", 
                _config.MqttBrokerHost, _config.MqttBrokerPort);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker");
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (IsConnected)
            {
                await _client.DisconnectAsync();
                _logger.LogInformation("Disconnected from MQTT broker");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from MQTT broker");
        }
    }

    public async Task PublishAsync(string topic, string payload)
    {
        try
        {
            if (!IsConnected)
            {
                _logger.LogWarning("Cannot publish - not connected to MQTT broker");
                return;
            }

            await _client.PublishAsync(topic, payload, QualityOfService.AtLeastOnceDelivery);
            _logger.LogDebug("Published message to topic {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}", topic);
        }
    }

    public async Task SubscribeAsync(string topic, Func<string, Task> messageHandler)
    {
        try
        {
            _client.OnMessageReceived += async (sender, args) =>
            {
                if (args.PublishMessage.Topic == topic)
                {
                    var payload = Encoding.UTF8.GetString(args.PublishMessage.Payload ?? Array.Empty<byte>());
                    await messageHandler(payload);
                }
            };

            await _client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery);
            _logger.LogInformation("Subscribed to topic {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to topic {Topic}", topic);
            throw;
        }
    }
}