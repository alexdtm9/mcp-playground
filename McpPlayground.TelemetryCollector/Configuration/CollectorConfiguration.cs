namespace McpPlayground.TelemetryCollector.Configuration;

public class CollectorConfiguration
{
    public string MqttBrokerHost { get; set; } = "localhost";
    public int MqttBrokerPort { get; set; } = 1883;
    public string TelemetryTopicPattern { get; set; } = "devices/+/telemetry";
    public string CommandTopicPattern { get; set; } = "devices/+/commands";
    public string StatusTopicPattern { get; set; } = "devices/+/status";
}