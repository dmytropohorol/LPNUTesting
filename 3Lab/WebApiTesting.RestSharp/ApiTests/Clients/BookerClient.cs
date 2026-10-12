using ApiTests.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiTests.Clients
{
    /// <summary>
    /// Клієнт сервісу restful-booker: авторизація та операції CRUD над
    /// ресурсом /booking.
    /// </summary>
    public class BookerClient : ApiClientBase
    {
        public const string BaseUrl = "https://restful-booker.herokuapp.com";

        /// <summary>Облікові дані з документації сервісу.</summary>
        public const string AdminUser = "admin";
        public const string AdminPassword = "password123";

        public BookerClient() : base(BaseUrl)
        {
        }

        // ----------------------------------------------------- службові ---

        /// <summary>GET /ping. Перевірка доступності сервісу.</summary>
        public RestResponse Ping()
        {
            return Send(new RestRequest("ping", Method.Get));
        }

        // -------------------------------------------------- авторизація ---

        /// <summary>POST /auth. Повертає маркер доступу на 15 символів.</summary>
        /// <remarks>
        /// Повтор дозволений явно, хоча метод POST загалом повторювати не
        /// можна: цей запит не створює ресурсу, а лише видає ще один маркер.
        /// Без повтору поодинокий збій мережі зривав би передумову одразу
        /// всіх сценаріїв, яким потрібна авторизація.
        /// </remarks>
        public RestResponse CreateToken(string username, string password)
        {
            var request = new RestRequest("auth", Method.Post);
            request.AddJsonBody(new { username = username, password = password });
            return Send(request, true);
        }

        // --------------------------------------------------------- читання ---

        /// <summary>GET /booking. Список номерів усіх бронювань.</summary>
        public RestResponse GetBookingIds()
        {
            return Send(Json(new RestRequest("booking", Method.Get)));
        }

        /// <summary>GET /booking з фільтром за іменем клієнта.</summary>
        public RestResponse GetBookingIdsByName(string firstName, string lastName)
        {
            var request = Json(new RestRequest("booking", Method.Get));
            request.AddQueryParameter("firstname", firstName);
            request.AddQueryParameter("lastname", lastName);
            return Send(request);
        }

        /// <summary>GET /booking/:id. Одне бронювання.</summary>
        public RestResponse GetBooking(int id)
        {
            return Send(Json(new RestRequest("booking/" + id, Method.Get)));
        }

        // -------------------------------------------------------- запис ---

        /// <summary>POST /booking. Створення бронювання, маркер не потрібен.</summary>
        public RestResponse CreateBooking(Booking booking)
        {
            var request = Json(new RestRequest("booking", Method.Post));
            request.AddJsonBody(booking);
            return Send(request);
        }

        /// <summary>PUT /booking/:id. Повна заміна бронювання.</summary>
        public RestResponse UpdateBooking(int id, Booking booking, string token)
        {
            var request = Json(new RestRequest("booking/" + id, Method.Put));
            Authorize(request, token);
            request.AddJsonBody(booking);
            return Send(request);
        }

        /// <summary>
        /// PATCH /booking/:id. Часткова зміна: передаються лише ті поля, які
        /// треба змінити, решта лишається без змін.
        /// </summary>
        public RestResponse PatchBooking(int id, JObject changes, string token)
        {
            var request = Json(new RestRequest("booking/" + id, Method.Patch));
            Authorize(request, token);
            request.AddStringBody(changes.ToString(), ContentType.Json);
            return Send(request);
        }

        /// <summary>DELETE /booking/:id. Видалення бронювання.</summary>
        public RestResponse DeleteBooking(int id, string token)
        {
            var request = new RestRequest("booking/" + id, Method.Delete);
            Authorize(request, token);
            return Send(request);
        }

        // ------------------------------------------------------ помічники ---

        /// <summary>
        /// Просить відповідь у JSON. Без цього заголовка сервіс віддає тіло в
        /// XML, і розбір відповіді не збігається з моделями.
        /// </summary>
        private static RestRequest Json(RestRequest request)
        {
            request.AddHeader("Accept", "application/json");
            return request;
        }

        /// <summary>
        /// Додає маркер доступу. Сервіс чекає його у заголовку Cookie, а не в
        /// Authorization: це відхід від звичного для REST способу, і саме через
        /// нього потрібен окремий метод замість вбудованого аутентифікатора
        /// RestSharp. Порожній маркер означає навмисно неавторизований запит.
        /// </summary>
        private static void Authorize(RestRequest request, string token)
        {
            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Cookie", "token=" + token);
        }
    }
}
