using SensorStateStats.Storage;
using System;
using SensorStateStats.Models;
using System.Threading.Tasks;

namespace Tests.SensorStateStats.Stabs
{
    class TestStatsCollection : IStatsCollection
    {
        public StatsSensorState GetLatestStatsRecord(Guid clientId, int sensorId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StoreHourlyStatsAsync(StatsSensorState statsRecord)
        {
            throw new NotImplementedException();
        }
    }
}
