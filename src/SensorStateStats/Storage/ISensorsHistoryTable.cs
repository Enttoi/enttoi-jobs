﻿using SensorStateStats.Models;
using System;
using System.Collections.Generic;

namespace SensorStateStats.Storage
{
    interface ISensorsHistoryTable : IStorageTable<SensorStateHistory>
    {
        List<SensorStateHistory> GetHourHistory(Guid clientId, int sensorId, DateTime from);
    }
}
