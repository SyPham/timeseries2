using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.AspNetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MqttHost
{
    public class Program
    {
       
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureWebHost(config =>
                {
                    config.UseKestrel(o =>
                    {
                        o.ListenAnyIP(8000, l => l.UseMqtt());
                        // o.ListenAnyIP(5005); // default http pipeline
                    });
                    //config.UseUrls("http://*:5001");
                });
        //private static IWebHost BuildWebHost(string[] args) =>
        //WebHost.CreateDefaultBuilder(args)
        //    .UseKestrel(o =>
        //    {
        //        o.ListenAnyIP(1883, l => l.UseMqtt());
        //        // o.ListenAnyIP(5005); // default http pipeline
        //    })
        //    .UseStartup<Startup>()
        //    .Build();
    }
}
