using Newtonsoft.Json;

namespace ApiTests.Models
{
    public class BookingId
    {
        [JsonProperty("bookingid")] public int Value { get; set; }
    }
}
