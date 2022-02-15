using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MQTT_SERVER.Models
{
    [Table("rawdata")]
    public class CycleTimes
    {
        public CycleTimes()
        {
        }
        [Column("id")]
        public int ID { get; set; }
        [Column("machineID")]
        public string MachineID { get; set; }
        public int RPM { get; set; }
        [Column("duration")]
        public int Duration { get; set; }
        [Column("sequence")]
        public int Sequence { get; set; }
        [Column("mixingInfoID")]
        public int MixingInfoID { get; set; }
        [Column("createddatetime")]
        public DateTime CreatedDateTime { get; set; }
    }
}
