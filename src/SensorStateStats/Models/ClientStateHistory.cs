using System;

namespace SensorStateStats.Models
{
    public class ClientStateHistory
    {
        public Guid ClientId { get; set; }

        public bool IsOnline { get; set; }

        public DateTime StateChangedTimestamp { get; set; }
    }
}
