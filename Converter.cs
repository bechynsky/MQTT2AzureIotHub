using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MQTT2AzureIotHub
{
    public class DeviceMessage
    {
        private ServerMessage _serverMessage = new ServerMessage();
        public DeviceMessage(string json)
        {
            JObject parser = JObject.Parse(json);
            JToken data = parser["state"]["reported"];
            
            _serverMessage.Temperature = float.Parse(data["temperature"].ToString());
            _serverMessage.Humidity = float.Parse(data["humidity"].ToString());

        }

        public string GetServerMessageJSON()
        {
            return JsonConvert.SerializeObject(_serverMessage);
        }
    }

    public class ServerMessage
    {
        [JsonProperty("h")]
        public float Humidity { get; set; }
        
        [JsonProperty("t")]
        public float Temperature { get; set; }
    }

}