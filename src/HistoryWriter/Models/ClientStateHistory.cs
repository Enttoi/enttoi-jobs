using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryWriter.Models
{
    public class ClientStateHistory : TableEntity
    {
        public Guid ClientId { get; set; }

        public bool IsOnline { get; set; }

        public DateTime StateChangedTimestamp { get; set; }

        public long PreviousStateDurationMs { get; set; }

        public ClientStateHistory(ClientStateMessage clientStateMessage)
        {
            if (clientStateMessage == null) throw new ArgumentNullException(nameof(clientStateMessage));

            var now = DateTime.UtcNow;            

            this.PartitionKey = clientStateMessage.ClientId.ToString();
            this.RowKey = clientStateMessage.Timestamp.Ticks.ToString();

            this.ClientId = clientStateMessage.ClientId;
            this.IsOnline = clientStateMessage.NewState;
            this.StateChangedTimestamp = clientStateMessage.Timestamp;
            this.PreviousStateDurationMs = clientStateMessage.PreviousStateDurationMs;
        }
    }
}
