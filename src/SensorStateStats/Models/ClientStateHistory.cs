using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SensorStateStats.Models
{
    public class ClientStateHistory : TableEntity
    {
        public Guid ClientId { get; set; }

        public bool IsOnline { get; set; }

        public DateTime StateChangedTimestamp { get; set; }
    }
}
