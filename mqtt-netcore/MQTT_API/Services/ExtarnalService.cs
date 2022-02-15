using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTT_API.Services
{
    public class ExtarnalService
    {
        private readonly IMqttClientService mqttClientService;
        public ExtarnalService(MqttClientServiceProvider provider)
        {
            mqttClientService = provider.MqttClientService;
        }
    }
}
