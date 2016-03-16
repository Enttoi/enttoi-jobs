using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace HistoryWriter.Models
{
    public class SensorStateHistory : TableEntity
    {
        public int SensorId { get; set; }

        public string SensorType { get; set; }

        public Guid ClientId { get; set; }

        public int State { get; set; }

        public DateTime StateChangedTimestamp { get; set; }

        public int PreviousState { get; set; }

        public long PreviousStateDurationMs { get; set; }


        public SensorStateHistory(SensorStateMessage sensorStateMessage)
        {
            if (sensorStateMessage == null) throw new ArgumentNullException(nameof(sensorStateMessage));
            
            var now = DateTime.UtcNow;

            this.PartitionKey = $"{sensorStateMessage.ClientId}-{sensorStateMessage.SensorId}";
            this.RowKey = sensorStateMessage.Timestamp.Ticks.ToString("d19");

            this.SensorId = sensorStateMessage.SensorId;
            this.SensorType = sensorStateMessage.SensorType;
            this.ClientId = sensorStateMessage.ClientId;

            this.StateChangedTimestamp = sensorStateMessage.Timestamp;
            this.State = sensorStateMessage.NewState;
            this.PreviousState = sensorStateMessage.PreviousState;
            this.PreviousStateDurationMs = sensorStateMessage.PreviousStateDurationMs;
        }
    }
}
