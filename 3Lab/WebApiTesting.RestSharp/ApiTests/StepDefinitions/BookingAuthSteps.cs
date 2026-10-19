using ApiTests.Clients;
using ApiTests.Models;
using ApiTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace ApiTests.StepDefinitions
{
    [Binding]
    public class BookingAuthSteps
    {
        private readonly BookerClient _booker;
        private readonly ApiContext _context;

        private AuthResult _auth;

        public BookingAuthSteps(BookerClient booker, ApiContext context)
        {
            _booker = booker;
            _context = context;
        }

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
            const string noToken = "";

            switch (operation)
            {
                case "replace":
                    _context.Response = _booker.UpdateBooking(_context.BookingId, TestData.ReplacementBooking(), noToken);
                    break;
                case "change":
                    var changes = new JObject();
                    changes["totalprice"] = 1;
                    _context.Response = _booker.PatchBooking(_context.BookingId, changes, noToken);
                    break;
                default:
                    _context.Response = _booker.DeleteBooking(_context.BookingId, noToken);
                    break;
            }
        }

        [Then(@"an access token of (\d+) characters is returned")]
        public void ThenAnAccessTokenIsReturned(int length)
        {
            Assert.AreEqual(length, _auth.Token?.Length ?? 0, "Маркер доступу не той: " + _auth.Token);
        }

        [Then(@"no token is returned")]
        public void ThenNoTokenIsReturned()
        {
            Assert.IsTrue(string.IsNullOrEmpty(_auth.Token), "Сервіс видав маркер за неправильних даних.");
        }

        [Then(@"the refusal reason is ""(.*)""")]
        public void ThenTheRefusalReasonIs(string expected)
        {
            Assert.AreEqual(expected, _auth.Reason);
        }

        [Then(@"the booking is unchanged")]
        public void ThenTheBookingIsUnchanged()
        {
            var actual = JsonConvert.DeserializeObject<Booking>(_booker.GetBooking(_context.BookingId).Content);

            Assert.IsNotNull(actual, "Бронювання стало недоступним після відмови у записі.");
            Assert.AreEqual(_context.SentBooking.FirstName, actual.FirstName, "Запит без маркера змінив імʼя.");
            Assert.AreEqual(_context.SentBooking.TotalPrice, actual.TotalPrice, "Запит без маркера змінив суму.");
        }

        // Сервіс відповідає кодом 200 і на неправильні дані, відмову видно лише
        // з поля reason у тілі. Відхилення описане у звіті.
        private void Authenticate(string username, string password)
        {
            _context.Response = _booker.CreateToken(username, password);
            _auth = JsonConvert.DeserializeObject<AuthResult>(_context.Response.Content);
        }
    }
}
