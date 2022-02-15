using MQTT_SERVER.hepler;
using MQTT_SERVER.Models;
using MQTT_SERVER.Topics;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace client
{
    class Program
    {
        private const string MQTTServerUrl = "10.4.4.224";
        private const int MQTTPort = 1886;
        private const string RPMTopic = "/hello/data";

        public static async Task Main()
        {
            MqttFactory factory = new MqttFactory();

            // Create a new MQTT client.            
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
            .WithTcpServer(MQTTServerUrl, MQTTPort)
            .Build();

            // Reconnecting event handler
            mqttClient.UseDisconnectedHandler(async e =>
            {
                MyConsole.Warning("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch
                {
                    MyConsole.Error("### RECONNECTING FAILED ###");
                }
            });

            // Message received event handler
            // Consuming messages
            mqttClient.UseApplicationMessageReceivedHandler( e =>
            {
                RecevicedTopic(e);
            });

            // Connected event handler
            mqttClient.UseConnectedHandler( e =>
            {
                // Subscribe to topics
                //await mqttClient.SubscribeAsync(MyTopic);
                //await mqttClient.SubscribeAsync(RPMTopic);
                //await mqttClient.SubscribeAsync(RSSITopic);
                //await mqttClient.SubscribeAsync(SettingTopic);

                // Subscribe all topics
                //await mqttClient.SubscribeAsync("#");
                MyConsole.Data($"### SUBSCRIBED TOPICS ###");
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
            if (mqttClient.IsConnected )
            {
                int dur = 1;

                await SetInterval(async () =>
             {
                 var data = new RPMTopic
                 {
                     r = Utilities.ToRandomInt(248, 260),
                     d = dur++,
                     i = 163
                 };
                 string payload = JsonConvert.SerializeObject(data);
                 Console.WriteLine($"Button {data.i}" + payload);
                 var message = new MqttApplicationMessageBuilder()
                 .WithTopic(RPMTopic)
                 .WithPayload(payload)
                 .Build();
                 await mqttClient.PublishAsync(message);
                 MyConsole.Info($"### RPM Topic SENT MESSAGE {Encoding.UTF8.GetString(message.Payload)} TO SERVER ");

             }, TimeSpan.FromSeconds(1));
            }

       
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
            MyConsole.Info("### RECEIVED APPLICATION MESSAGE ###");
            MyConsole.Info($"+ Topic = {e.ApplicationMessage.Topic}");
            MyConsole.Info($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            MyConsole.Info($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            MyConsole.Info($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
        }

    }
}
