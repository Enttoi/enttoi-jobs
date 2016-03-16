using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SensorStateStats.Models
{
    public class SensorStateHistory : TableEntity
    {
        public int SensorId { get; set; }

        public string SensorType { get; set; }

        public Guid ClientId { get; set; }

        public int State { get; set; }

        public DateTime StateChangedTimestamp { get; set; }
    }
}
