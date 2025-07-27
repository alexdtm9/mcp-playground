using McpPlayground.Shared.Data;
using McpPlayground.TelemetryCollector.Configuration;
using McpPlayground.TelemetryCollector.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire defaults
builder.AddServiceDefaults();

// Configure database
builder.Services.AddDbContextPool<McpDbContext>(opt => 
    opt.UseNpgsql(builder.Configuration.GetConnectionString("postgresdb")));

// Configure MQTT settings
builder.Services.Configure<CollectorConfiguration>(config =>
{
    config.MqttBrokerHost = "localhost";
    config.MqttBrokerPort = 34629;
});

// Add the background service
builder.Services.AddHostedService<TelemetryCollectorService>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<McpDbContext>();
    await dbContext.Database.MigrateAsync();
}

await host.RunAsync();