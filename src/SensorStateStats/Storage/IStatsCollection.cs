using SensorStateStats.Models;
using System;

namespace SensorStateStats.Storage
{
    public interface IStatsCollection
    {
        StatsSensorState GetLatestStatsRecord(Guid clientId, int sensorId);

        void StoreHourlyStats(StatsSensorState statsRecord);
    }
}
