using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT_SERVER.hepler
{
    public class MongoDbSettings : IMongoDbSettings
    {
        public string IoTCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
    public interface IMongoDbSettings
    {
        string IoTCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
