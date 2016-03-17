using SensorStateStats.Models;
using System;
using System.Collections.Generic;

namespace SensorStateStats.Storage
{
    interface ISensorsHistoryTable
    {
        List<SensorStateHistory> GetHourHistory(Guid clientId, int sensorId, DateTime from);
    }
}
