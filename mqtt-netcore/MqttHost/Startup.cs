using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.Extensions;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Server;

namespace MqttHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services
              .AddHostedMqttServer(mqttServer => mqttServer.WithDefaultEndpoint())
              .AddMqttConnectionHandler()
              .AddConnections();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMqtt("/mqtt");
            });

            app.UseMqttServer(async server =>
            {

                server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e =>
                {
                    Console.WriteLine($"Client {e.ClientId} disconnected event fired.");
                });
                server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e =>
                {
                    Console.WriteLine($"Client {e.ClientId} connected event fired.");
                });
                server.StartedHandler = new MqttServerStartedHandlerDelegate(async args =>
                {
                    var frameworkName = GetType().Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?
                        .FrameworkName;

                    var msg = new MqttApplicationMessageBuilder()
                        .WithPayload($"Mqtt hosted on {frameworkName} is awesome")
                        .WithTopic("message");

                    while (true)
                    {
                        try
                        {
                            await server.PublishAsync(msg.Build());
                            msg.WithPayload($"Mqtt hosted on {frameworkName} is still awesome at {DateTime.Now}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        finally
                        {
                            await Task.Delay(TimeSpan.FromSeconds(2));
                        }
                    }
                });
               // await CreateMqttClientAsync();
            });

         
        }

        private async Task CreateMqttClientAsync()
        {

            MqttFactory factory = new MqttFactory();
            // Create a new MQTT client.            
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                    .WithWebSocketServer("localhost")
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
            mqttClient.UseApplicationMessageReceivedHandler( e =>
            {
                // RecevicedTopic(e);
                if (e.ApplicationMessage.Topic == "my/topic")
                {
                    //var payload = e.ApplicationMessage.Payload;
                    //string result = System.Text.Encoding.UTF8.GetString(payload);
                    //var rpmTopic = JsonConvert.DeserializeObject<RPMTopic>(result);

                    Console.WriteLine($"RPM data = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                }
            });
            // Connected event handler
            mqttClient.UseConnectedHandler( async e =>
            {
                // Subscribe to topics
                await mqttClient.SubscribeAsync("my/topic");

                // Subscribe all topics
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("#").Build());
            });
            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });
            // Try to connect to MQTT server
            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception exception)
            {
                Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
            }

        }
    }
}
