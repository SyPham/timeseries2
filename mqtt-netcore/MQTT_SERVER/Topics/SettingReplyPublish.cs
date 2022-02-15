using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT_SERVER.Topics
{
   public class SettingReplyPublish
    {
        public int id { get; set; }
        public int minRPM { get; set; }
        public int maxRPM { get; set; }
        public int timer { get; set; }
        public string tp { get; set; } = "set";
    }
}
