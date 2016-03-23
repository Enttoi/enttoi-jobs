using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SensorStateStats.Models
{
    public class StatsSensorState
    {
        [JsonProperty(PropertyName = "id")]
        public string DocumentId { get; set; }

        [JsonProperty(PropertyName = "clientId")]
        public Guid ClientId { get; set; }

        [JsonProperty(PropertyName = "sensorId")]
        public int SensorId { get; set; }

        [JsonProperty(PropertyName = "clientPrevHistRowKey")]
        public string ClientPreviousHistoryRecordRowKey { get; set; }

        [JsonProperty(PropertyName = "sensorPrevHistRowKey")]
        public string SensorPreviousHistoryRecordRowKey { get; set; }

        [JsonProperty(PropertyName = "timeStampHourResolution")]
        public DateTime TimeStampHourResolution { get; set; }

        [JsonProperty(PropertyName = "states")]
        public Dictionary<int, long> States { get; set; }

        public StatsSensorState()
        {
            DocumentId = Guid.NewGuid().ToString("D");
        }
    }
}
