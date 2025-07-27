namespace McpPlayground.Simulator.Models;

public class DeviceCommand
{
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}