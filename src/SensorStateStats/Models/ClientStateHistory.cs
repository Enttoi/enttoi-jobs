using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SensorStateStats.Models
{
    public class ClientStateHistory : TableEntity, IComparable<ClientStateHistory>
    {
        public Guid ClientId { get; set; }

        public bool IsOnline { get; set; }

        public DateTime StateChangedTimestamp { get; set; }

        public int CompareTo(ClientStateHistory other)
        {
            return this.StateChangedTimestamp.CompareTo(other.StateChangedTimestamp);
        }


    }
}
