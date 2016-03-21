using SensorStateStats.Storage;
using System;
using System.Collections.Generic;
using SensorStateStats.Models;

namespace Tests.SensorStateStats.Stabs
{
    class TestSensorsHistoryTable : ISensorsHistoryTable
    {
        public SensorStateHistory Get(string partitionKey, string rowKey)
        {
            throw new NotImplementedException();
        }

        public List<SensorStateHistory> GetHourHistory(Guid clientId, int sensorId, DateTime from)
        {
            throw new NotImplementedException();
        }

        public SensorStateHistory GetOldestHistoryRecord(Guid clientId, int sensorId)
        {
            throw new NotImplementedException();
        }
    }
}
