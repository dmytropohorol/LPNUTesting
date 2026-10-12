using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiTests.Models
{
    /// <summary>
    /// Прогноз погоди, ресурс /v1/forecast сервісу Open-Meteo.
    /// </summary>
    /// <remarks>
    /// Склад полів current і daily залежить від запиту, тому значення та
    /// одиниці зберігаються словниками: так модель не треба правити щоразу,
    /// коли тест просить іншу змінну погоди.
    /// </remarks>
    public class Forecast
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("elevation")]
        public double Elevation { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("utc_offset_seconds")]
        public int UtcOffsetSeconds { get; set; }

        [JsonProperty("current_units")]
        public Dictionary<string, string> CurrentUnits { get; set; }

        [JsonProperty("current")]
        public Dictionary<string, object> Current { get; set; }

        [JsonProperty("daily_units")]
        public Dictionary<string, string> DailyUnits { get; set; }

        [JsonProperty("daily")]
        public Dictionary<string, List<object>> Daily { get; set; }
    }
}
