using JobsCommon;
using JobsCommon.Logger;
using Microsoft.WindowsAzure.Storage.Table;
using SensorStateStats.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SensorStateStats.Storage
{
    class ClientsHistoryTable : BaseStorageTable<ClientStateHistory>, IClientsHistoryTable
    {
        public ClientsHistoryTable(ILogger logger) : base(logger)
        {
            _table = _client.GetTableReference(Configurations.HISTORY_TABLE_CLIENTS_STATE);
        }

        public ClientStateHistory GetOldestHistoryRecord(Guid clientId)
        {
            var query = new StorageRangeQuery<ClientStateHistory>(
                clientId.ToString(), 
                0.ToString("d19"), 
                DateTime.UtcNow.Ticks.ToString("d19"));

            return query.GetTopOne(_table);
        }

        public List<ClientStateHistory> GetHourHistory(Guid clientId, DateTime from)
        {
            var ticksFrom = from.Ticks.ToString("d19");
            var ticksTo = from.AddHours(1).Ticks.ToString("d19");

            var query = new StorageRangeQuery<ClientStateHistory>(clientId.ToString(), ticksFrom, ticksTo);
            return query.GetFullResult(_table).ToList();
        }
    }
}
