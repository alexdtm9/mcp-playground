namespace McpPlayground.Simulator.Configuration;

public class DeviceConfiguration
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string MqttBrokerHost { get; set; } = "localhost";
    public int MqttBrokerPort { get; set; } = 1883;
    public TimeSpan PublishInterval { get; set; } = TimeSpan.FromSeconds(15);
    public string CommandTopic { get; set; } = string.Empty;
    public string TelemetryTopic { get; set; } = string.Empty;
    public string StatusTopic { get; set; } = string.Empty;
}