using McpPlayground.Simulator.Configuration;
using McpPlayground.Simulator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace McpPlayground.Simulator.Services;

public interface ISimulationEngine
{
    DeviceState CurrentState { get; }
    void UpdateState();
    void ProcessCommand(DeviceCommand command);
}

public class SimulationEngine : ISimulationEngine
{
    private readonly SimulationConfiguration _config;
    private readonly ILogger<SimulationEngine> _logger;
    private readonly Random _random = new();
    private readonly DeviceState _state;

    public DeviceState CurrentState => _state;

    public SimulationEngine(IOptions<SimulationConfiguration> options, ILogger<SimulationEngine> logger)
    {
        _config = options.Value;
        _logger = logger;
        _state = new DeviceState
        {
            Temperature = _config.TemperatureSetpoint,
            Humidity = _config.HumiditySetpoint,
            PowerConsumption = 100.0 // Base consumption in watts
        };
    }

    public void UpdateState()
    {
        switch (_state.Mode)
        {
            case OperatingMode.Normal:
                UpdateNormalMode();
                break;
            case OperatingMode.ForcedCooling:
                UpdateForcedCooling();
                break;
            case OperatingMode.ForcedDehumidifying:
                UpdateForcedDehumidifying();
                break;
            case OperatingMode.EnergySaving:
                UpdateEnergySavingMode();
                break;
        }

        UpdatePowerConsumption();
        _state.LastUpdate = DateTime.UtcNow;
    }

    private void UpdateNormalMode()
    {
        // Update temperature
        var tempChange = _random.NextDouble() < _config.IncreaseChance
            ? _random.NextDouble() * _config.MaxTemperatureChange
            : -_random.NextDouble() * _config.MaxTemperatureChange;

        _state.Temperature += tempChange;

        // Update humidity
        var humidityChange = _random.NextDouble() < _config.IncreaseChance
            ? _random.NextDouble() * _config.MaxHumidityChange
            : -_random.NextDouble() * _config.MaxHumidityChange;

        _state.Humidity += humidityChange;

        // Check thresholds and activate systems
        if (_state.Temperature > _config.TemperatureMaxThreshold)
        {
            _state.CompressorActive = true;
            _logger.LogInformation("Temperature exceeded threshold ({Temp:F1}°C). Compressor activated.", 
                _state.Temperature);
        }

        if (_state.Humidity > _config.HumidityMaxThreshold)
        {
            _state.DehumidifierActive = true;
            _logger.LogInformation("Humidity exceeded threshold ({Humidity:F1}%). Dehumidifier activated.", 
                _state.Humidity);
        }

        // If systems are active, force values down
        if (_state.CompressorActive)
        {
            _state.Temperature -= _random.NextDouble() * _config.MaxTemperatureChange * 2;
            if (_state.Temperature <= _config.TemperatureSetpoint)
            {
                _state.CompressorActive = false;
                _logger.LogInformation("Temperature normalized. Compressor deactivated.");
            }
        }

        if (_state.DehumidifierActive)
        {
            _state.Humidity -= _random.NextDouble() * _config.MaxHumidityChange * 2;
            if (_state.Humidity <= _config.HumiditySetpoint)
            {
                _state.DehumidifierActive = false;
                _logger.LogInformation("Humidity normalized. Dehumidifier deactivated.");
            }
        }
    }

    private void UpdateForcedCooling()
    {
        _state.CompressorActive = true;
        _state.Temperature -= _random.NextDouble() * _config.MaxTemperatureChange * 1.5;

        if (_state.Temperature <= _config.TemperatureMinThreshold)
        {
            _state.Mode = OperatingMode.Normal;
            _state.CompressorActive = false;
            _logger.LogInformation("Temperature reached minimum threshold. Switching to normal mode.");
        }
    }

    private void UpdateForcedDehumidifying()
    {
        _state.DehumidifierActive = true;
        _state.Humidity -= _random.NextDouble() * _config.MaxHumidityChange * 1.5;

        if (_state.Humidity <= _config.HumidityMinThreshold)
        {
            _state.Mode = OperatingMode.Normal;
            _state.DehumidifierActive = false;
            _logger.LogInformation("Humidity reached minimum threshold. Switching to normal mode.");
        }
    }

    private void UpdateEnergySavingMode()
    {
        // In energy saving mode, allow wider temperature/humidity ranges
        var extendedTempThreshold = _config.TemperatureMaxThreshold + 2.0;
        var extendedHumidityThreshold = _config.HumidityMaxThreshold + 5.0;

        // Similar logic to normal mode but with extended thresholds
        var tempChange = _random.NextDouble() < 0.5
            ? _random.NextDouble() * _config.MaxTemperatureChange * 0.5
            : -_random.NextDouble() * _config.MaxTemperatureChange * 0.5;

        _state.Temperature += tempChange;

        if (_state.Temperature > extendedTempThreshold && !_state.CompressorActive)
        {
            _state.CompressorActive = true;
        }
        else if (_state.Temperature <= _config.TemperatureSetpoint + 1)
        {
            _state.CompressorActive = false;
        }

        // Update humidity with minimal intervention
        _state.Humidity += (_random.NextDouble() - 0.5) * _config.MaxHumidityChange * 0.3;
    }

    private void UpdatePowerConsumption()
    {
        var basePower = 100.0;
        
        if (_state.CompressorActive)
            basePower += 500.0;
        
        if (_state.DehumidifierActive)
            basePower += 200.0;

        if (_state.Mode == OperatingMode.EnergySaving)
            basePower *= 0.7;

        // Add some random variation
        _state.PowerConsumption = basePower + (_random.NextDouble() - 0.5) * 20;
    }

    public void ProcessCommand(DeviceCommand command)
    {
        _logger.LogInformation("Processing command: {Command}", command.Command);

        switch (command.Command.ToUpperInvariant())
        {
            case "ACTIVATE_COMPRESSOR":
                _state.Mode = OperatingMode.ForcedCooling;
                break;
            case "ACTIVATE_DEHUMIDIFIER":
                _state.Mode = OperatingMode.ForcedDehumidifying;
                break;
            case "ENERGY_SAVING":
                _state.Mode = OperatingMode.EnergySaving;
                break;
            case "NORMAL_MODE":
                _state.Mode = OperatingMode.Normal;
                _state.CompressorActive = false;
                _state.DehumidifierActive = false;
                break;
            default:
                _logger.LogWarning("Unknown command: {Command}", command.Command);
                break;
        }
    }
}