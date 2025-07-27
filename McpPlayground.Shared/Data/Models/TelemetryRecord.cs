namespace McpPlayground.Shared.Data.Models;

public class TelemetryRecord
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double PowerConsumption { get; set; }
    public bool CompressorActive { get; set; }
    public bool DehumidifierActive { get; set; }
    public string OperatingMode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}