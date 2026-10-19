using Newtonsoft.Json;

namespace ApiTests.Models
{
    public class AuthResult
    {
        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("reason")] public string Reason { get; set; }
    }
}
