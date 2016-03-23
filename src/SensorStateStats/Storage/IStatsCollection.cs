using SensorStateStats.Models;
using System;
using System.Threading.Tasks;

namespace SensorStateStats.Storage
{
    public interface IStatsCollection
    {
        StatsSensorState GetLatestStatsRecord(Guid clientId, int sensorId);

        Task<bool> StoreHourlyStatsAsync(StatsSensorState statsRecord);
    }
}
