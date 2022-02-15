using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using TimeSeries2.Data;
using Document = TimeSeries2.Data.Document;

namespace TimeSeries2.Models
{
    [BsonCollection("timeseries")]
    public class TimeSeries: Document
    {
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
        [BsonElement("value")]
        public int Value { get; set; }
    }
}
