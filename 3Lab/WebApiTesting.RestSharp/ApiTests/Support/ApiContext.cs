using RestSharp;

namespace ApiTests.Support
{
    // Стан, який кроки одного сценарію передають один одному.
    // SpecFlow створює новий екземпляр на кожен сценарій.
    public class ApiContext
    {
        public RestResponse Response { get; set; }
        public string Token { get; set; }
        public int BookingId { get; set; }
        public Models.Booking SentBooking { get; set; }
    }
}
