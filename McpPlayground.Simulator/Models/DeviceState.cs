namespace McpPlayground.Simulator.Models;

public class DeviceState
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double PowerConsumption { get; set; }
    public bool CompressorActive { get; set; }
    public bool DehumidifierActive { get; set; }
    public OperatingMode Mode { get; set; } = OperatingMode.Normal;
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}

public enum OperatingMode
{
    Normal,
    ForcedCooling,
    ForcedDehumidifying,
    EnergySaving
}