﻿using SensorStateStats.Models;
using System;
using System.Collections.Generic;

namespace SensorStateStats.Storage
{
    public interface IClientsHistoryTable : IStorageTable<ClientStateHistory>
    {
        ClientStateHistory GetOldestHistoryRecord(Guid clientId);

        List<ClientStateHistory> GetHourHistory(Guid clientId, DateTime from);        
    }
}
