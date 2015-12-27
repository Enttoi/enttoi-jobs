using Newtonsoft.Json;
using System;

namespace ClientsState.Models
{
    public class Client
    {
        [JsonProperty(PropertyName = "id")]
        public Guid ClientId { get; set; }

        public string[] Tags { get; set; }

        public bool IsOnline { get; set; }

        public DateTime IsOnlineChanged { get; set; }

        public Sensor[] Sensors { get; set; }
    }
}