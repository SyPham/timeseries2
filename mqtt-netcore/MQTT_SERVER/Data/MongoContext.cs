using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MQTT_SERVER.hepler;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT_SERVER.Data
{
    public interface IIMongoContext
    {
        IMongoDatabase Database { get; }
    }
    public class MongoContext : DbContext
    {
        public IMongoDatabase Database { get; set; }

        public MongoContext(IOptions<MongoDbSettings> configuration)
        {
            var _mongoClient = new MongoClient(configuration.Value.ConnectionString);
            Database = _mongoClient.GetDatabase(configuration.Value.DatabaseName);
            // _db = _mongoClient.GetDatabase(configuration.Value.DatabaseName);
        }

    }
}
