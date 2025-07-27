namespace McpPlayground.Simulator.Configuration;

public class SimulationConfiguration
{
    public double TemperatureSetpoint { get; set; }
    public double HumiditySetpoint { get; set; }
    public double TemperatureMaxThreshold { get; set; }
    public double HumidityMaxThreshold { get; set; }
    public double TemperatureMinThreshold { get; set; }
    public double HumidityMinThreshold { get; set; }
    public double IncreaseChance { get; set; }
    public double MaxTemperatureChange { get; set; }
    public double MaxHumidityChange { get; set; }
}