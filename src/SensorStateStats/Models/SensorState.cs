using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorStateStats.Models
{
    public class SensorState
    {
        [JsonProperty(PropertyName = "state")]
        public int State { get; set; }

        [JsonProperty(PropertyName = "duration")]
        public long Duration { get; set; }
    }
}
