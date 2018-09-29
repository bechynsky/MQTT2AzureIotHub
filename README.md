# MQTT2AzureIoTHub
Simple Field Gateway reading data from local MQTT broker and send them to Azure IoT Hub.

# iot-hub explorer

```
iothub-explorer monitor-events BC01 --login "<connection string for service endpoint>"
```

# MQTT monitor

```
mosquitto_sub -t "#" -v
```