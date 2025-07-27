namespace McpPlayground.Simulator.Models;

public class DeviceTelemetry
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public SensorData Sensors { get; set; } = new();
    public ActuatorStatus Actuators { get; set; } = new();
    public string OperatingMode { get; set; } = string.Empty;
}

public class SensorData
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double PowerConsumption { get; set; }
}

public class ActuatorStatus
{
    public bool Compressor { get; set; }
    public bool Dehumidifier { get; set; }
}