
using JobsCommon;
using JobsCommon.Logger;
using SensorStateStats.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SensorStateStats.Storage
{
    class SensorsHistoryTable : BaseStorageTable<SensorStateHistory>, ISensorsHistoryTable
    {
        public SensorsHistoryTable(ILogger logger) : base(logger)
        {
            _table = _client.GetTableReference(Configurations.HISTORY_TABLE_SENSORS_STATE);
        }

        public List<SensorStateHistory> GetHourHistory(Guid clientId, int sensorId, DateTime from)
        {
            var ticksFrom = from.Ticks.ToString("d19");
            var ticksTo = from.AddHours(1).Ticks.ToString("d19");

            var sensorsQuery = new StorageRangeQuery<SensorStateHistory>($"{clientId}-{sensorId}", ticksFrom, ticksTo);
            return sensorsQuery.GetFullResult(_table).ToList();
        }
    }
}
