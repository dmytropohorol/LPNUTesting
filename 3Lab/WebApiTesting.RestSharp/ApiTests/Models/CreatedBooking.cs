using Newtonsoft.Json;

namespace ApiTests.Models
{
    /// <summary>
    /// Відповідь на POST /booking. Сервіс повертає не саме бронювання, а
    /// обгортку з його номером, тоді як GET /booking/:id повертає бронювання
    /// без обгортки. Несумісність форматів описана у звіті.
    /// </summary>
    public class CreatedBooking
    {
        [JsonProperty("bookingid")]
        public int BookingId { get; set; }

        [JsonProperty("booking")]
        public Booking Booking { get; set; }
    }
}
