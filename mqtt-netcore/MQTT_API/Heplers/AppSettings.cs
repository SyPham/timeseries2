using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTT_API.Heplers
{
    public class AppSettings
    {
        public string URL { get; set; }
        public string SignalrUri { get; set; }
        public string Host { get; set; }
        public string RedisHost { get; set; }
        public int Port { get; set; }
        public string Token { get; set; }
        public string applicationUrl { get; set; }
        public string[] CorsPolicy { get; set; }
    }
}
