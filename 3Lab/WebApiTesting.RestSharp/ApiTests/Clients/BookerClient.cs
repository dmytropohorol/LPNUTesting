using ApiTests.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiTests.Clients
{
    public class BookerClient : ApiClientBase
    {
        public const string BaseUrl = "https://restful-booker.herokuapp.com";
        public const string AdminUser = "admin";
        public const string AdminPassword = "password123";

        public BookerClient() : base(BaseUrl)
        {
        }

        public RestResponse Ping()
        {
            return Send(new RestRequest("ping", Method.Get));
        }

        public RestResponse CreateToken(string username, string password)
        {
            var request = new RestRequest("auth", Method.Post);
            request.AddJsonBody(new { username = username, password = password });
            return Send(request);
        }

        public RestResponse GetBookingIds()
        {
            return Send(Json(new RestRequest("booking", Method.Get)));
        }

        public RestResponse GetBookingIdsByName(string firstName, string lastName)
        {
            var request = Json(new RestRequest("booking", Method.Get));
            request.AddQueryParameter("firstname", firstName);
            request.AddQueryParameter("lastname", lastName);
            return Send(request);
        }

        public RestResponse GetBooking(int id)
        {
            return Send(Json(new RestRequest("booking/" + id, Method.Get)));
        }

        public RestResponse CreateBooking(Booking booking)
        {
            var request = Json(new RestRequest("booking", Method.Post));
            request.AddJsonBody(booking);
            return Send(request);
        }

        public RestResponse UpdateBooking(int id, Booking booking, string token)
        {
            var request = Json(new RestRequest("booking/" + id, Method.Put));
            Authorize(request, token);
            request.AddJsonBody(booking);
            return Send(request);
        }

        public RestResponse PatchBooking(int id, JObject changes, string token)
        {
            var request = Json(new RestRequest("booking/" + id, Method.Patch));
            Authorize(request, token);
            request.AddStringBody(changes.ToString(), ContentType.Json);
            return Send(request);
        }

        public RestResponse DeleteBooking(int id, string token)
        {
            var request = new RestRequest("booking/" + id, Method.Delete);
            Authorize(request, token);
            return Send(request);
        }

        // Без цього заголовка сервіс віддає тіло в XML.
        private static RestRequest Json(RestRequest request)
        {
            request.AddHeader("Accept", "application/json");
            return request;
        }

        // Сервіс чекає маркер у Cookie, а не в Authorization, тому вбудований
        // аутентифікатор RestSharp не підходить. Порожній маркер означає запит без нього.
        private static void Authorize(RestRequest request, string token)
        {
            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Cookie", "token=" + token);
        }
    }
}
