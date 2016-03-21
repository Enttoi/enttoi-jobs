using SensorStateStats.Storage;
using System;
using SensorStateStats.Models;

namespace Tests.SensorStateStats.Stabs
{
    class TestStatsCollection : IStatsCollection
    {
        public StatsSensorState GetLatestStatsRecord(Guid clientId, int sensorId)
        {
            throw new NotImplementedException();
        }

        public void StoreHourlyStats(StatsSensorState statsRecord)
        {
            throw new NotImplementedException();
        }
    }
}
