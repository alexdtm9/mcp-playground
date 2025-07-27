using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var postgres = builder.AddPostgres("postgres");
var postgresdb = postgres.AddDatabase("postgresdb");

// Add Mosquitto MQTT broker
var mosquitto = builder.AddContainer("mosquitto", "eclipse-mosquitto")
    .WithBindMount("./mosquitto.conf", "/mosquitto/config/mosquitto.conf")
    .WithEndpoint(targetPort: 1883, port: 34629, name: "mqtt");

var simulator = builder.AddProject<McpPlayground_Simulator>("simulator")
    .WithReferenceRelationship(mosquitto);

// Add the telemetry collector
var collector = builder.AddProject<McpPlayground_TelemetryCollector>("telemetry-collector")
    .WithReferenceRelationship(mosquitto).WaitFor(mosquitto)
    .WithReference(postgresdb).WaitFor(postgresdb);

// Add Server
var server = builder.AddProject<McpPlayground_McpServer>("mcp-server");
    

builder.Build().Run();