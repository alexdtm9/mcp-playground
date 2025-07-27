using HiveMQtt.Client;
using HiveMQtt.Client.Options;

namespace McpPlayground.Simulator;

public class Device
{
    private readonly HiveMQClient client;

    public Device()
    {
        var mqqtBrokerHost = "localhost";
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker(mqqtBrokerHost)
            .WithPort(34629)
            .Build();
        
        client = new HiveMQClient(options);
    }
    
    public async Task ConnectAsync()
    {
        try
        {
            await client.ConnectAsync();
            Console.WriteLine("Connected to MQTT broker.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
        }
    }
    
    public async Task DisconnectAsync()
    {
        try
        {
            await client.DisconnectAsync();
            Console.WriteLine("Disconnected from MQTT broker.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to disconnect from MQTT broker: {ex.Message}");
        }
    }
    
    public async Task RunSimulationLoopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting simulation loop...");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            // Simulate some work
            await Task.Delay(1000, cancellationToken);
            
            // Publish a message to the MQTT broker
            await client.PublishAsync("simulator/topic", "Hello from McpPlayground.Simulator");
            Console.WriteLine("Published message to MQTT broker.");
        }
        
        Console.WriteLine("Simulation loop ended.");
    }
}