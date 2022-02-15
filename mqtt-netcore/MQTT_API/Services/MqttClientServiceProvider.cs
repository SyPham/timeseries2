using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTT_API.Services
{
    public class MqttClientServiceProvider
    {
        public readonly IMqttClientService MqttClientService;

        public MqttClientServiceProvider(IMqttClientService mqttClientService)
        {
            MqttClientService = mqttClientService;
        }
    }
}
