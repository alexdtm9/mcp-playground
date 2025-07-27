using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add Mosquitto MQTT broker
var mosquitto = builder.AddContainer("mosquitto", "eclipse-mosquitto")
    .WithBindMount("./mosquitto.conf", "/mosquitto/config/mosquitto.conf")
    .WithEndpoint(targetPort: 1883, port: 34629, name: "mqtt");

var simulator = builder.AddProject<McpPlayground_Simulator>("simulator")
    .WithReferenceRelationship(mosquitto);
    

builder.Build().Run();