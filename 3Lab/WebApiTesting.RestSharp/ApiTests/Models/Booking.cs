using Newtonsoft.Json;

namespace ApiTests.Models
{
    public class Booking
    {
        [JsonProperty("firstname")] public string FirstName { get; set; }
        [JsonProperty("lastname")] public string LastName { get; set; }
        [JsonProperty("totalprice")] public int TotalPrice { get; set; }
        [JsonProperty("depositpaid")] public bool DepositPaid { get; set; }
        [JsonProperty("bookingdates")] public BookingDates BookingDates { get; set; }
        [JsonProperty("additionalneeds")] public string AdditionalNeeds { get; set; }
    }
}
