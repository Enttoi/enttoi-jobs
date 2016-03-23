using SensorStateStats.Storage;
using System;
using System.Collections.Generic;
using SensorStateStats.Models;

namespace Tests.SensorStateStats.Stabs
{
    class TestClientsHistoryTable : IClientsHistoryTable
    {
        public ClientStateHistory Get(string partitionKey, string rowKey)
        {
            throw new NotImplementedException();
        }

        public List<ClientStateHistory> GetHourHistory(Guid clientId, DateTime from)
        {
            throw new NotImplementedException();
        }

        public ClientStateHistory GetOldestHistoryRecord(Guid clientId)
        {
            throw new NotImplementedException();
        }
    }
}
