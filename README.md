## Setup
Test
You just need to run the AspireHost. First time you may need
to manually restart the collector service so that the database has time to create.

## Interacting with the Simulator via the Command Line
First install the cli package: https://mosquitto.org/download/

### Listen to all device messages:
```bash
# Subscribe to all device topics (use # wildcard)
mosquitto_sub -h localhost -p 34629 -t "devices/SIM-001/#" -v
```

### Listen to specific topics:
```bash
# Only telemetry data
mosquitto_sub -h localhost -p 34629 -t "devices/SIM-001/telemetry" -v

# Only status updates
mosquitto_sub -h localhost -p 34629 -t "devices/SIM-001/status" -v

# Only commands (to verify they're being sent)
mosquitto_sub -h localhost -p 34629 -t "devices/SIM-001/commands" -v
```

### Activate Compressor (Forced Cooling):
```bash
mosquitto_pub -h localhost -p 34629 -t "devices/SIM-001/commands" -m '{"command":"ACTIVATE_COMPRESSOR","timestamp":"2025-01-27T10:00:00Z"}'
```

### Activate Dehumidifier:
```bash
mosquitto_pub -h localhost -p 34629 -t "devices/SIM-001/commands" -m '{"command":"ACTIVATE_DEHUMIDIFIER","timestamp":"2025-01-27T10:00:00Z"}'
```

### Enable Energy Saving Mode:
```bash
mosquitto_pub -h localhost -p 34629 -t "devices/SIM-001/commands" -m '{"command":"ENERGY_SAVING","timestamp":"2025-01-27T10:00:00Z"}'
```

### Return to Normal Mode:
```bash
mosquitto_pub -h localhost -p 34629 -t "devices/SIM-001/commands" -m '{"command":"NORMAL_MODE","timestamp":"2025-01-27T10:00:00Z"}'
```
