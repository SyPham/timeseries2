namespace MyMqttNet.Models
{
    public class KestrelSettings
    {
        public int MqttPipeLinePort { get; set; }
        public int HttpPipeLinePort { get; set; }
        public int HttpsPipeLinePort { get; set; }
    }
}