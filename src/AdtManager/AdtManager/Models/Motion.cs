using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdtManager.Models
{
    public class Motion
    {
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Telemetry> Telemetry { get; set; }
    }
}
