using SensorStateStats.Models;
using System;
using System.Collections.Generic;

namespace SensorStateStats.Storage
{
    interface IClientsHistoryTable
    {
        ClientStateHistory GetOldestClientHistory(Guid clientId);

        List<ClientStateHistory> GetHourHistory(Guid clientId, DateTime from);
    }
}
