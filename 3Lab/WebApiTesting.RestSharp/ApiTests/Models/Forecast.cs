using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiTests.Models
{
    // Склад current і daily залежить від запиту, тому словники.
    public class Forecast
    {
        [JsonProperty("latitude")] public double Latitude { get; set; }
        [JsonProperty("longitude")] public double Longitude { get; set; }
        [JsonProperty("timezone")] public string Timezone { get; set; }
        [JsonProperty("current_units")] public Dictionary<string, string> CurrentUnits { get; set; }
        [JsonProperty("current")] public Dictionary<string, object> Current { get; set; }
        [JsonProperty("daily")] public Dictionary<string, List<object>> Daily { get; set; }
    }
}
