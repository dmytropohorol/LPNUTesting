using Newtonsoft.Json;

namespace ApiTests.Models
{
    /// <summary>Дати заїзду та виїзду у форматі yyyy-MM-dd.</summary>
    /// <remarks>
    /// Дати зберігаються рядками, а не DateTime. Серіалізатор перевів би
    /// DateTime у повну позначку часу з часовим поясом, і порівняння
    /// очікуваного значення з фактичним залежало б від поясу машини.
    /// </remarks>
    public class BookingDates
    {
        [JsonProperty("checkin")]
        public string CheckIn { get; set; }

        [JsonProperty("checkout")]
        public string CheckOut { get; set; }
    }
}
