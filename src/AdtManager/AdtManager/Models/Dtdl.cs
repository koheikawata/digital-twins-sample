using Azure.DigitalTwins.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtManager.Models
{
    public class Dtdl
    {
        [JsonProperty("@id")]
        public string Id { get; set; }
    }
}
