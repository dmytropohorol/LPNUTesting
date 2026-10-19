using Newtonsoft.Json;

namespace ApiTests.Models
{
    public class ApiError
    {
        [JsonProperty("error")] public bool Error { get; set; }
        [JsonProperty("reason")] public string Reason { get; set; }
    }
}
