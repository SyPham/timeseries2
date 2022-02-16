using System;
using System.Collections.Generic;
using System.Text;

namespace MyMqttNet.Models
{ 
   public class timeseries
    {
        public DateTime timestamp { get; set; }
        public int value { get; set; }
    }
}
