using Newtonsoft.Json;

namespace ApiTests.Models
{
    /// <summary>
    /// Повідомлення про помилку сервісу Open-Meteo. Приходить разом зі
    /// статусом 400 і містить причину відмови у полі reason.
    /// </summary>
    public class ApiError
    {
        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
