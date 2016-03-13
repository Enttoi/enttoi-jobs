using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryWriter.Models
{
    public class SensorStateMessage
    {
        public int SensorId { get; set; }
        
        public string SensorType { get; set; }

        public Guid ClientId { get; set; }

        public int NewState { get; set; }

        public int PreviousState { get; set; }

        public long PreviousStateDurationMs { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
