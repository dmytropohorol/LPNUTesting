using Newtonsoft.Json;

namespace ApiTests.Models
{
    /// <summary>
    /// Бронювання готельного номера, ресурс /booking сервісу restful-booker.
    /// </summary>
    /// <remarks>
    /// Імена полів у JSON повністю в нижньому регістрі, тому кожне поле має
    /// явний атрибут JsonProperty: покладатися на автоматичне зведення імен
    /// не варто, бо воно залежить від налаштувань серіалізатора.
    /// </remarks>
    public class Booking
    {
        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

        [JsonProperty("totalprice")]
        public int TotalPrice { get; set; }

        [JsonProperty("depositpaid")]
        public bool DepositPaid { get; set; }

        [JsonProperty("bookingdates")]
        public BookingDates BookingDates { get; set; }

        [JsonProperty("additionalneeds")]
        public string AdditionalNeeds { get; set; }
    }
}
