using Newtonsoft.Json;

namespace ApiTests.Models
{
    /// <summary>Елемент списку, який повертає GET /booking.</summary>
    public class BookingId
    {
        [JsonProperty("bookingid")]
        public int Value { get; set; }
    }
}
