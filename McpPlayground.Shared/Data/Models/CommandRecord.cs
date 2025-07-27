namespace McpPlayground.Shared.Data.Models;

public class CommandRecord
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string? Parameters { get; set; } // JSON string
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}