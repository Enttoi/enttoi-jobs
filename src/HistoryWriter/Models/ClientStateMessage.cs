using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryWriter.Models
{
    public class ClientStateMessage
    {
        public Guid ClientId { get; set; }

        public bool NewState { get; set; }

        public long PreviousStateDurationMs { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
