namespace McpPlayground.TelemetryCollector.DTOs;

public class TelemetryDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public SensorDataDto Sensors { get; set; } = new();
    public ActuatorStatusDto Actuators { get; set; } = new();
    public string OperatingMode { get; set; } = string.Empty;
}