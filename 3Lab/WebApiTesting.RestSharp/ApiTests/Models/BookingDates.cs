using Newtonsoft.Json;

namespace ApiTests.Models
{
    // Дати рядками yyyy-MM-dd: DateTime серіалізувався б із часовим поясом,
    // і порівняння залежало б від налаштувань машини.
    public class BookingDates
    {
        [JsonProperty("checkin")] public string CheckIn { get; set; }
        [JsonProperty("checkout")] public string CheckOut { get; set; }
    }
}
