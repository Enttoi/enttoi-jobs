using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace HistoryWriter.Models
{
    public class SensorStateHistory : TableEntity
    {
        public int SensorId { get; set; }

        public string SensorType { get; set; }

        public Guid ClientId { get; set; }

        public int NewState { get; set; }

        public DateTime NewStateTimestamp { get; set; }

        public int PreviousState { get; set; }

        public long PreviousStateDurationMs { get; set; }


        public SensorStateHistory(SensorStateMessage sensorStateMessage)
        {
            if (sensorStateMessage == null) throw new ArgumentNullException(nameof(sensorStateMessage));
            
            var now = DateTime.UtcNow;

            this.PartitionKey = $"{now.Year}-{now.Month}-{now.Day}-{sensorStateMessage.ClientId}-{sensorStateMessage.SensorId}";
            this.RowKey = now.Ticks.ToString();

            this.SensorId = sensorStateMessage.SensorId;
            this.SensorType = sensorStateMessage.SensorType;
            this.ClientId = sensorStateMessage.ClientId;
            this.NewStateTimestamp = sensorStateMessage.Timestamp;
            this.PreviousState = sensorStateMessage.PreviousState;
            this.PreviousStateDurationMs = sensorStateMessage.PreviousStateDurationMs;
        }
    }
}
