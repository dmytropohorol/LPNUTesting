using Newtonsoft.Json;

namespace ApiTests.Models
{
    /// <summary>
    /// Відповідь на POST /auth. Заповнене лише одне з двох полів: за правильних
    /// даних приходить Token, за неправильних Reason.
    /// </summary>
    public class AuthResult
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
