using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT_SERVER.Topics
{
   public class RPMTopic
    {
        public int i { get; set; } // machineID
        public int r { get; set; }// RPM
        public int d { get; set; } // duration
    }
}
