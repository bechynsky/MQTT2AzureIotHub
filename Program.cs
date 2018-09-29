using System;
using System.Collections.Generic;

using Microsoft.Azure.Devices.Client;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;

namespace MQTT2AzureIotHub
{
    class Program
    {
        private static MqttClient _mqttClient = null;
        private static IConfiguration _configuration { get; set; }
        private static Dictionary<string, DeviceClient> _deviceConnectionStrings = new Dictionary<string, DeviceClient>();
        private const string _DEVICEID = "DeviceId=";
        private static AppConfig _ac = null;


        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("connection.json");
            
            _configuration = builder.Build();

            _ac = _configuration.GetSection("AppConfig").Get<AppConfig>();

            foreach (string connectionString in _ac.ConnectionStrings)
            {
                // get device name
                int i1 = connectionString.IndexOf(_DEVICEID, StringComparison.InvariantCultureIgnoreCase);
                if (i1 < 0)
                {
                    continue;
                }
                int i2 = connectionString.IndexOf(";", i1, StringComparison.InvariantCultureIgnoreCase);
                string deviceid = connectionString.Substring(i1 + _DEVICEID.Length, i2 - i1 - _DEVICEID.Length);
                
                DeviceClient dc = DeviceClient.CreateFromConnectionString(connectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                _deviceConnectionStrings.Add(deviceid.ToLower(), dc);

                Console.WriteLine(deviceid);
            }

            _mqttClient = new MqttClient(_ac.MQTTServer);
            _mqttClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

            _mqttClient.Connect(_ac.MQTTClientName);
            
            if (_mqttClient.IsConnected)
            {
                _mqttClient.Subscribe(new string[] { _ac.MQTTSubscribeTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Console.WriteLine("MQTT connected");
            }

            while (true) {}
        }

        private static async void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            string[] topicParts = e.Topic.Split('/');
            Console.WriteLine(e.Topic);
            string value = System.Text.Encoding.Default.GetString(e.Message);
            Console.WriteLine(value);

            DeviceClient dc = null;

            if (_deviceConnectionStrings.TryGetValue(topicParts[_ac.MQTTDeviceNameIndex], out dc))
            {
                Message eventMessage = new Message(e.Message);
                // send message
                await dc.SendEventAsync(eventMessage);
            }
        }
    }

    class AppConfig
    {
        public string MQTTServer { get; set; }
        public string MQTTClientName { get; set; }
        public string MQTTSubscribeTopic { get; set; }
        public int MQTTDeviceNameIndex { get; set; }
        public string[] ConnectionStrings { get; set; }
    }
}
