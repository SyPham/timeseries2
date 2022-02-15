using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT_SERVER.hepler
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
        public string DefaultConnection { get; set; }
        public string MongoConnection { get; set; }
        public string MongoDb { get; set; }
    }
}
