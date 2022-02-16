using MongoDB.Driver;
using MQTT_SERVER.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTSERVER
{
    class Program
    {
        private const string MyTopic = "/hello/data";
        private const string MQTTServerUrl = "localhost";
        private const int MQTTPort = 1883;
        static async Task Main(string[] args)
        {
            // Configure MQTT server.
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionBacklog(100)
                .WithDefaultEndpointPort(MQTTPort);

            var mqttServer = new MqttFactory().CreateMqttServer();
            await mqttServer.StartAsync(optionsBuilder.Build());

            MqttFactory factory = new MqttFactory();
            // Create a new MQTT client.            
            var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("Henry")
            .WithWebSocketServer("localhost:1886/mqtt")
            .Build();


            // mongodb
            MongoClient _mongoClient = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase _db = _mongoClient.GetDatabase("timeseries");
            IMongoCollection<timeseries> _rawDatasCollection = _db.GetCollection<timeseries>("timeseries");

            // Reconnecting event handler
            mqttClient.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });

            // Message received event handler
            // Consuming messages
            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                if (e.ApplicationMessage.Topic == MyTopic)
                {
                    var payload = e.ApplicationMessage.Payload;
                    string result = Encoding.UTF8.GetString(payload);
                    var timeseries = JsonConvert.DeserializeObject<timeseries>(result);
                    // save db
                    await _rawDatasCollection.InsertOneAsync(timeseries);
                }
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
            });
            // Connected event handler
            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(MyTopic).Build());

                // Subscribe all topics
                // await mqttClient.SubscribeAsync("#");

                Console.WriteLine("### SUBSCRIBED ###");
            });

            // Try to connect to MQTT server
            await mqttClient.ConnectAsync(options, CancellationToken.None);

            // Publishing messages  
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(MyTopic)
                .WithPayload("Hello mqtt")
                .Build();
            await mqttClient.PublishAsync(message);

            Console.WriteLine($"### SENT MESSAGE {Encoding.UTF8.GetString(message.Payload)} TO SERVER ");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
            await mqttServer.StopAsync();
        }

    }
}
