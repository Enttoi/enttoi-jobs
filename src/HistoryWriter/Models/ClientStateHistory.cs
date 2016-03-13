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

        public bool NewState { get; set; }

        public long PreviousStateDurationMs { get; set; }

        public DateTime NewStateTimestamp { get; set; }

        public ClientStateHistory(ClientStateMessage clientStateMessage)
        {
            if (clientStateMessage == null) throw new ArgumentNullException(nameof(clientStateMessage));

            var now = DateTime.UtcNow;            

            this.PartitionKey = $"{now.Year}-{now.Month}-{now.Day}-{clientStateMessage.ClientId}";
            this.RowKey = now.Ticks.ToString();

            this.ClientId = clientStateMessage.ClientId;
            this.NewState = clientStateMessage.NewState;
            this.PreviousStateDurationMs = clientStateMessage.PreviousStateDurationMs;
            this.NewStateTimestamp = clientStateMessage.Timestamp;
        }
    }
}
