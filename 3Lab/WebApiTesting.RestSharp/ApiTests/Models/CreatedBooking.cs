using Newtonsoft.Json;

namespace ApiTests.Models
{
    // POST /booking повертає обгортку з номером, GET /booking/:id сам запис.
    public class CreatedBooking
    {
        [JsonProperty("bookingid")] public int BookingId { get; set; }
        [JsonProperty("booking")] public Booking Booking { get; set; }
    }
}
