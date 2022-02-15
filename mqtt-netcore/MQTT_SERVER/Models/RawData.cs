using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization;
namespace MQTT_SERVER.Models
{
    public class RawData
    {
        public RawData()
        {
            createddatetime = this.createddatetime.ToLocalTime();
        }

        [BsonId]
        public ObjectId _id { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int machineID { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int RPM { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int duration { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int sequence { get; set; }
        [BsonElement]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime createddatetime { get; set; }
    }
}
