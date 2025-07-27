namespace McpPlayground.TelemetryCollector.DTOs;

public class CommandDto
{
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public DateTime Timestamp { get; set; }
}