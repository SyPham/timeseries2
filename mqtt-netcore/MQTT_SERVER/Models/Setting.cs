using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace MQTT_SERVER.Models
{
   public class Setting
    {
        [BsonId]
        public ObjectId _id { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int machineID { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int standardRPM { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int minRPM { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int startBuzzerAfter { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int stopwatch { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int timer { get; set; }

    }
}
