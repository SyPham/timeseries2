using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MQTT_SERVER.hepler
{
    public static class ConfigExtension
    {
        /// <summary>
        ///     Reads the configuration.
        /// </summary>
        /// <param name="currentPath">The current path.</param>
        /// <returns>A <see cref="Config" /> object.</returns>
        private static AppSettings ReadAppSettingsConfiguration(this string currentPath)
        {
            var filePath = $"{currentPath}\\appsettings.json";
            AppSettings config = null;
            // ReSharper disable once InvertIf
            if (File.Exists(filePath))
            {
                using var r = new StreamReader(filePath);
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<AppSettings>(json);
            }
            return config;
        }
        /// <summary>
        ///     Reads the configuration.
        /// </summary>
        /// <param name="currentPath">The current path.</param>
        /// <returns>A <see cref="Config" /> object.</returns>
        private static ConnectionStrings ReadConnectionStringConfiguration(this  string currentPath)
        {
            var filePath = $"{currentPath}\\appsettings.json";
            ConnectionStrings config = null;
            // ReSharper disable once InvertIf
            if (File.Exists(filePath))
            {
                using var r = new StreamReader(filePath);
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<ConnectionStrings>(json);
            }

            return config;
        }
    }
}
