using MQTT_SERVER.Data;
using MQTT_SERVER.Models;
using MQTT_SERVER.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT_SERVER.Repositories.Implementation
{
    public class CycleTimesRepository : IoTRepositoryBase<CycleTimes>, ICycleTimesRepository
    {
        public CycleTimesRepository(IIMongoContext context) : base(context)
        {
        }
    }
}
