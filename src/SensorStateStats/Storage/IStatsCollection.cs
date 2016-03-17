using SensorStateStats.Models;
using System;

namespace SensorStateStats.Storage
{
    interface IStatsCollection
    {
        StatsSensorState GetLatestStatsRecord(Guid clientId, int sensorId);
    }
}
