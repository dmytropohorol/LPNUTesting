using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    [Binding]
    public class BookingCrudSteps
    {
        private readonly BookerClient _booker;
        private readonly ApiContext _context;

        public BookingCrudSteps(BookerClient booker, ApiContext context)
        {
            _booker = booker;
            _context = context;
        }

        [Given(@"the booking service is available")]
        public void GivenTheServiceIsAvailable()
        {
            // Сервіс відповідає на ping кодом 201, а не 200. Відхилення описане у звіті.
            Assert.AreEqual(HttpStatusCode.Created, _booker.Ping().StatusCode, "Сервіс restful-booker недоступний.");
        }

        [Given(@"I have an access token")]
        public void GivenIHaveAnAccessToken()
        {
            var auth = Deserialize<AuthResult>(_booker.CreateToken(BookerClient.AdminUser, BookerClient.AdminPassword));

            Assert.IsFalse(string.IsNullOrEmpty(auth?.Token), "Не вдалося отримати маркер доступу.");
            _context.Token = auth.Token;
        }

        [Given(@"a created booking")]
        public void GivenACreatedBooking()
        {
            WhenICreateABooking();
            ThenTheBookingIsCreatedAndHasANumber();
        }

        // Номер запамʼятовується тут, а не в кроці Then: сценарій без кроку
        // перевірки створення інакше працював би з бронюванням номер 0.
        [When(@"I create a booking")]
        public void WhenICreateABooking()
        {
            _context.SentBooking = TestData.ValidBooking();
            _context.Response = _booker.CreateBooking(_context.SentBooking);

            var created = Deserialize<CreatedBooking>(_context.Response);
            if (created != null)
                _context.BookingId = created.BookingId;
        }

        [When(@"I create a booking with the first name only")]
        public void WhenICreateAnIncompleteBooking()
        {
            _context.Response = _booker.CreateBooking(new Booking { FirstName = "OnlyName" });
        }

        [When(@"I read the created booking")]
        public void WhenIReadTheCreatedBooking()
        {
            _context.Response = _booker.GetBooking(_context.BookingId);
        }

        [When(@"I read a booking that does not exist")]
        public void WhenIReadAMissingBooking()
        {
            _context.Response = _booker.GetBooking(TestData.MissingBookingId);
        }

        [When(@"I read the list of booking numbers")]
        public void WhenIReadTheListOfBookingNumbers()
        {
            _context.Response = _booker.GetBookingIds();
        }

        [When(@"I search bookings by the customer name")]
        public void WhenISearchBookingsByName()
        {
            _context.Response = _booker.GetBookingIdsByName(_context.SentBooking.FirstName, _context.SentBooking.LastName);
        }

        [When(@"I replace the booking with other data")]
        public void WhenIReplaceTheBooking()
        {
            _context.SentBooking = TestData.ReplacementBooking();
            _context.Response = _booker.UpdateBooking(_context.BookingId, _context.SentBooking, _context.Token);
        }

        [When(@"I change only the total price to (\d+)")]
        public void WhenIChangeOnlyTheTotalPrice(int price)
        {
            var changes = new JObject();
            changes["totalprice"] = price;
            _context.Response = _booker.PatchBooking(_context.BookingId, changes, _context.Token);

            _context.SentBooking.TotalPrice = price;
        }

        [When(@"I delete the booking")]
        public void WhenIDeleteTheBooking()
        {
            _context.Response = _booker.DeleteBooking(_context.BookingId, _context.Token);
        }

        [When(@"I send a (replace|delete) request to a booking that does not exist")]
        public void WhenISendARequestToAMissingBooking(string operation)
        {
            int id = TestData.MissingBookingId;

            _context.Response = operation == "replace"
                ? _booker.UpdateBooking(id, TestData.ReplacementBooking(), _context.Token)
                : _booker.DeleteBooking(id, _context.Token);
        }

        [Then(@"the booking is created and has a number")]
        public void ThenTheBookingIsCreatedAndHasANumber()
        {
            Assert.AreEqual(HttpStatusCode.OK, _context.Response.StatusCode);

            var created = Deserialize<CreatedBooking>(_context.Response);

            Assert.IsTrue(created.BookingId > 0, "Сервіс не повернув номера бронювання.");
            AssertSameBooking(_context.SentBooking, created.Booking);
        }

        [Then(@"the booking matches the data I sent")]
        [Then(@"the booking matches the replacement data")]
        [Then(@"the other fields keep the values I sent")]
        public void ThenTheBookingMatchesTheSentData()
        {
            Assert.AreEqual(HttpStatusCode.OK, _context.Response.StatusCode);
            AssertSameBooking(_context.SentBooking, Deserialize<Booking>(_context.Response));
        }

        [Then(@"the booking is gone")]
        public void ThenTheBookingIsGone()
        {
            Assert.AreEqual(HttpStatusCode.NotFound, _booker.GetBooking(_context.BookingId).StatusCode,
                "Видалене бронювання все ще читається.");
        }

        [Then(@"the list contains the created booking number")]
        public void ThenTheListContainsTheCreatedBookingNumber()
        {
            Assert.AreEqual(HttpStatusCode.OK, _context.Response.StatusCode);

            var ids = Deserialize<List<BookingId>>(_context.Response);

            Assert.IsTrue(ids.Any(item => item.Value == _context.BookingId),
                "У списку немає бронювання " + _context.BookingId + ".");
        }

        [Then(@"the total price equals (\d+)")]
        public void ThenTheTotalPriceEquals(int expected)
        {
            Assert.AreEqual(HttpStatusCode.OK, _context.Response.StatusCode);
            Assert.AreEqual(expected, Deserialize<Booking>(_context.Response).TotalPrice, "Часткова зміна не записала нову суму.");
        }

        private static void AssertSameBooking(Booking expected, Booking actual)
        {
            Assert.IsNotNull(actual, "У відповіді немає бронювання.");
            Assert.AreEqual(expected.FirstName, actual.FirstName, "Не збігається імʼя.");
            Assert.AreEqual(expected.LastName, actual.LastName, "Не збігається прізвище.");
            Assert.AreEqual(expected.TotalPrice, actual.TotalPrice, "Не збігається сума.");
            Assert.AreEqual(expected.DepositPaid, actual.DepositPaid, "Не збігається ознака депозиту.");
            Assert.AreEqual(expected.AdditionalNeeds, actual.AdditionalNeeds, "Не збігаються побажання.");
            Assert.AreEqual(expected.BookingDates.CheckIn, actual.BookingDates.CheckIn, "Не збігається дата заїзду.");
            Assert.AreEqual(expected.BookingDates.CheckOut, actual.BookingDates.CheckOut, "Не збігається дата виїзду.");
        }

        // На кодах 404 і 405 сервіс віддає простий текст, а не JSON.
        private static T Deserialize<T>(RestResponse response) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content ?? "");
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
