using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private const string MQTTServerUrl = "localhost";
        private const int MQTTPort = 1886;
        private const string RPMTopic = "/hello/data";

        public static async Task Main()
        {
            MqttFactory factory = new MqttFactory();

            // Create a new MQTT client.            
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithClientId("Henry")
                .WithWebSocketServer("ws://localhost:5000/mqtt")
                .WithCleanSession()
            //.WithTcpServer(MQTTServerUrl, MQTTPort)
            .Build();

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
            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                RecevicedTopic(e);
            });

            // Connected event handler
            mqttClient.UseConnectedHandler(e =>
            {
                // Subscribe to topics
                //await mqttClient.SubscribeAsync(MyTopic);
                //await mqttClient.SubscribeAsync(RPMTopic);
                //await mqttClient.SubscribeAsync(RSSITopic);
                //await mqttClient.SubscribeAsync(SettingTopic);

                // Subscribe all topics
                //await mqttClient.SubscribeAsync("#");
                Console.WriteLine($"### SUBSCRIBED TOPICS ###");
            });
            // Create a Timer object that knows to call our TimerCallback
            // method once every 2000 milliseconds.
            // Try to connect to MQTT server
            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception exception)
            {
                Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
            }
            if (mqttClient.IsConnected)
            {

                await SetInterval(async () =>
                {
                    var data = new
                    {
                        value = ToRandomInt(248, 260),
                        timestamp = DateTime.Now
                    };
                    string payload = JsonConvert.SerializeObject(data);
                    var message = new MqttApplicationMessageBuilder()
                    .WithTopic(RPMTopic)
                    .WithPayload(payload)
                    .Build();
                    await mqttClient.PublishAsync(message);
                    Console.WriteLine($"### RPM Topic SENT MESSAGE {Encoding.UTF8.GetString(message.Payload)} TO SERVER ");

                }, TimeSpan.FromSeconds(5));
            }


        }
        public static int ToRandomInt(int minValue = 0, int maxValue = 100)
        {
            Random r = new Random();
            int rInt = r.Next(minValue, maxValue); //for ints
            return rInt;
        }
        public static async Task SetInterval(Action action, TimeSpan timeout)
        {
            await Task.Delay(timeout).ConfigureAwait(false);

            action();
            // Force a garbage collection to occur for this demo.
            GC.Collect();
            await SetInterval(action, timeout);
        }
        private static void RecevicedTopic(MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
        }

    }
}
