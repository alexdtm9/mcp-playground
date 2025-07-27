using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace McpPlayground.Simulator;

public class DeviceSimulatorService : BackgroundService
{
    private readonly DeviceSimulator _deviceSimulator;
    private readonly ILogger<DeviceSimulatorService> _logger;

    public DeviceSimulatorService(DeviceSimulator deviceSimulator, ILogger<DeviceSimulatorService> logger)
    {
        _deviceSimulator = deviceSimulator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _deviceSimulator.StartAsync(stoppingToken);
            await _deviceSimulator.RunSimulationLoopAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in device simulator");
            throw;
        }
        finally
        {
            await _deviceSimulator.StopAsync();
        }
    }
}