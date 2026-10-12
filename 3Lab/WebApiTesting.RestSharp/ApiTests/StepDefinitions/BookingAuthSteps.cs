using ApiTests.Clients;
using ApiTests.Models;
using ApiTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using TechTalk.SpecFlow;

namespace ApiTests.StepDefinitions
{
    /// <summary>
    /// Кроки сценаріїв авторизації з файлу BookingAuth.feature.
    /// </summary>
    /// <remarks>
    /// Кроки перевірки доступності сервісу та кроки читання бронювання тут не
    /// повторюються: SpecFlow шукає реалізацію кроку по всій збірці, тому
    /// обидва файли сценаріїв користуються одним набором кроків.
    /// </remarks>
    [Binding]
    public class BookingAuthSteps
    {
        /// <summary>Довжина маркера доступу, який видає сервіс.</summary>
        private const int TokenLength = 15;

        private readonly BookerClient _booker;
        private readonly ApiContext _context;

        private AuthResult _auth;

        public BookingAuthSteps(BookerClient booker, ApiContext context)
        {
            _booker = booker;
            _context = context;
        }

        // ------------------------------------------------------------- When ---

        [When(@"I request a token with the correct credentials")]
        public void WhenIRequestATokenWithCorrectCredentials()
        {
            Authenticate(BookerClient.AdminUser, BookerClient.AdminPassword);
        }

        [When(@"I request a token with the password ""(.*)""")]
        public void WhenIRequestATokenWithPassword(string password)
        {
            Authenticate(BookerClient.AdminUser, password);
        }

        [When(@"I request a token with the user name ""(.*)""")]
        public void WhenIRequestATokenWithUserName(string username)
        {
            Authenticate(username, BookerClient.AdminPassword);
        }

        [When(@"I send a (replace|change|delete) request without a token")]
        public void WhenISendAWriteRequestWithoutAToken(string operation)
        {
            // Порожній маркер означає запит без заголовка з маркером.
            const string noToken = "";

            switch (operation)
            {
                case "replace":
                    _context.Response = _booker.UpdateBooking(
                        _context.BookingId, TestData.ReplacementBooking(), noToken);
                    break;

                case "change":
                    var changes = new JObject();
                    changes["totalprice"] = 1;
                    _context.Response = _booker.PatchBooking(
                        _context.BookingId, changes, noToken);
                    break;

                case "delete":
                    _context.Response = _booker.DeleteBooking(_context.BookingId, noToken);
                    break;

                default:
                    Assert.Fail("Невідома операція: " + operation);
                    break;
            }
        }

        // ------------------------------------------------------------- Then ---

        [Then(@"an access token of (\d+) characters is returned")]
        public void ThenAnAccessTokenIsReturned(int length)
        {
            Assert.IsNotNull(_auth, "Відповідь не є JSON. " + _booker.ExchangeLog);
            Assert.IsFalse(string.IsNullOrEmpty(_auth.Token),
                "Сервіс не повернув маркера доступу. " + _booker.ExchangeLog);
            Assert.AreEqual(length, _auth.Token.Length,
                "Довжина маркера не та, що очікувалася: " + _auth.Token);
        }

        [Then(@"no token is returned")]
        public void ThenNoTokenIsReturned()
        {
            Assert.IsNotNull(_auth, "Відповідь не є JSON. " + _booker.ExchangeLog);
            Assert.IsTrue(string.IsNullOrEmpty(_auth.Token),
                "Сервіс видав маркер за неправильних даних. " + _booker.ExchangeLog);
        }

        [Then(@"the refusal reason is ""(.*)""")]
        public void ThenTheRefusalReasonIs(string expected)
        {
            Assert.IsNotNull(_auth, "Відповідь не є JSON. " + _booker.ExchangeLog);
            Assert.AreEqual(expected, _auth.Reason,
                "Причина відмови не та, що очікувалася. " + _booker.ExchangeLog);
        }

        [Then(@"the booking is unchanged")]
        public void ThenTheBookingIsUnchanged()
        {
            RestResponse read = _booker.GetBooking(_context.BookingId);

            Assert.AreEqual(200, (int)read.StatusCode,
                "Бронювання стало недоступним після відмови у записі. " + _booker.ExchangeLog);

            var actual = JsonConvert.DeserializeObject<Booking>(read.Content);
            Booking sent = _context.SentBooking;

            Assert.IsNotNull(actual, "Відповідь не є JSON. " + _booker.ExchangeLog);
            Assert.AreEqual(sent.FirstName, actual.FirstName,
                "Запит без маркера змінив імя клієнта.");
            Assert.AreEqual(sent.TotalPrice, actual.TotalPrice,
                "Запит без маркера змінив суму.");
        }

        // -------------------------------------------------------- помічники ---

        private void Authenticate(string username, string password)
        {
            _context.Response = _booker.CreateToken(username, password);

            // Сервіс відповідає кодом 200 і на правильні, і на неправильні
            // дані. Відмова розпізнається лише за полем reason у тілі,
            // а не за кодом відповіді. Відхилення зафіксоване у звіті.
            try
            {
                _auth = JsonConvert.DeserializeObject<AuthResult>(_context.Response.Content);
            }
            catch (JsonException)
            {
                _auth = null;
            }
        }
    }
}
