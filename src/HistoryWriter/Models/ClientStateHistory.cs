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

        public ClientStateHistory(ClientStateMessage clientStateMessage)
        {
            if (clientStateMessage == null) throw new ArgumentNullException(nameof(clientStateMessage));

            var now = DateTime.UtcNow;            

            this.PartitionKey = $"{now.Year}-{now.Month}-{now.Day}-{clientStateMessage.ClientId}";
            this.RowKey = clientStateMessage.Timestamp.Ticks.ToString();

            this.ClientId = clientStateMessage.ClientId;
            this.IsOnline = clientStateMessage.NewState;
            this.StateChangedTimestamp = clientStateMessage.Timestamp;
        }
    }
}
