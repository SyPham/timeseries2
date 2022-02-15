// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using Serilog;
using SimpleMqttServer.Heplers;
using Microsoft.Extensions.Logging;
using Data.Repositories.Interfaces;
using Data.Repositories.Implementation;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Data;
using Microsoft.EntityFrameworkCore;

namespace MqttServer
{
  
    /// <summary>
    ///     The main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        ///     The main method that starts the service.
        /// </summary>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static async Task Main()
        {
            var services = ConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            await serviceProvider.GetService<App>().RunAsync();

        }
        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            var conn = "Server=.;Database=mqtt;MultipleActiveResultSets=true;User Id=sa;Password=shc@1234";
            services.AddDbContext<DataContext>(option => option.UseSqlServer(
                    conn, x => x.MigrationsAssembly("Data")));
            var config = LoadConfiguration();
            services.AddSingleton(config);

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<App>();
            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
        
    }
}