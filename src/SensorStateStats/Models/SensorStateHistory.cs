using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorStateStats.Models
{
    public class SensorStateHistory
    {
        public int SensorId { get; set; }

        public string SensorType { get; set; }

        public Guid ClientId { get; set; }

        public int State { get; set; }

        public DateTime StateChangedTimestamp { get; set; }
    }
}
