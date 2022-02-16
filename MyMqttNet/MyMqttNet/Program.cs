#region Using Imports

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MQTTnet.AspNetCore.Extensions;
using System.Linq;
using System.Net;

#endregion Using Imports

namespace MyMqttNet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Load the AppSettings file so we can load in the configured Kestrel Port Settings to host upon
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build();

            var mqttPipeLinePort = int.Parse(config["AppSettings:KestrelSettings:MqttPipeLinePort"]);
            var httpPipeLinePort = int.Parse(config["AppSettings:KestrelSettings:HttpPipeLinePort"]);
            var httpsPipeLinePort = int.Parse(config["AppSettings:KestrelSettings:HttpsPipeLinePort"]);

            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(kestrelServerOptions =>
                {
                    string HostName = Dns.GetHostName();

                    // Configure the port for the MQTT PipeLine
                    kestrelServerOptions.ListenAnyIP(mqttPipeLinePort, listenOptions => listenOptions.UseMqtt());
                    // Configure the port for the Default HTTP PipeLine
                    kestrelServerOptions.ListenAnyIP(httpPipeLinePort);
                 
                }).UseStartup<Startup>();
            });
        }
    }
}