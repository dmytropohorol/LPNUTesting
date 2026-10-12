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
    /// <summary>
    /// Кроки сценаріїв CRUD з файлу BookingCrud.feature.
    /// </summary>
    /// <remarks>
    /// Кроки не працюють із RestSharp напряму: адреси ресурсів і формат тіла
    /// знає лише клієнт. Так зміна контракту API зачіпає один файл, а не
    /// кожен сценарій.
    /// </remarks>
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

        // ------------------------------------------------------------ Given ---

        [Given(@"the booking service is available")]
        public void GivenTheServiceIsAvailable()
        {
            RestResponse response = _booker.Ping();

            // Сервіс відповідає на перевірку доступності кодом 201, хоча для
            // читання очікувався б 200. Відхилення описане у звіті.
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
                "Сервіс restful-booker недоступний. " + _booker.ExchangeLog);
        }

        [Given(@"I have an access token")]
        public void GivenIHaveAnAccessToken()
        {
            RestResponse response = _booker.CreateToken(
                BookerClient.AdminUser, BookerClient.AdminPassword);

            var auth = Deserialize<AuthResult>(response);

            Assert.IsNotNull(auth, "Відповідь на запит маркера не є JSON. " + _booker.ExchangeLog);
            Assert.IsFalse(string.IsNullOrEmpty(auth.Token),
                "Не вдалося отримати маркер доступу. " + _booker.ExchangeLog);

            _context.Token = auth.Token;
        }

        [Given(@"a created booking")]
        public void GivenACreatedBooking()
        {
            WhenICreateABooking();
            ThenTheBookingIsCreatedAndHasANumber();
        }

        // ------------------------------------------------------------- When ---

        [When(@"I create a booking")]
        public void WhenICreateABooking()
        {
            _context.SentBooking = TestData.ValidBooking();
            _context.Response = _booker.CreateBooking(_context.SentBooking);

            RememberCreatedBooking();
        }

        [When(@"I create a booking with the first name only")]
        public void WhenICreateAnIncompleteBooking()
        {
            // Тіло навмисно неповне: обовязкових полів у ньому немає.
            var incomplete = new Booking { FirstName = "OnlyName" };
            _context.Response = _booker.CreateBooking(incomplete);
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
            _context.Response = _booker.GetBookingIdsByName(
                _context.SentBooking.FirstName, _context.SentBooking.LastName);
        }

        [When(@"I replace the booking with other data")]
        public void WhenIReplaceTheBooking()
        {
            _context.SentBooking = TestData.ReplacementBooking();
            _context.Response = _booker.UpdateBooking(
                _context.BookingId, _context.SentBooking, _context.Token);
        }

        [When(@"I change only the total price to (\d+)")]
        public void WhenIChangeOnlyTheTotalPrice(int price)
        {
            var changes = new JObject();
            changes["totalprice"] = price;

            _context.Response = _booker.PatchBooking(
                _context.BookingId, changes, _context.Token);

            // Очікування сценарію: змінилася лише сума, решта полів та сама.
            _context.SentBooking.TotalPrice = price;
        }

        [When(@"I delete the booking")]
        public void WhenIDeleteTheBooking()
        {
            _context.Response = _booker.DeleteBooking(_context.BookingId, _context.Token);

            // Бронювання вже немає, тож прибирати його після сценарію не треба.
            _context.CreatedBookingIds.Remove(_context.BookingId);
        }

        [When(@"I send a (replace|delete) request to a booking that does not exist")]
        public void WhenISendARequestToAMissingBooking(string operation)
        {
            int id = TestData.MissingBookingId;

            switch (operation)
            {
                case "replace":
                    _context.Response = _booker.UpdateBooking(
                        id, TestData.ReplacementBooking(), _context.Token);
                    break;

                case "delete":
                    _context.Response = _booker.DeleteBooking(id, _context.Token);
                    break;

                default:
                    Assert.Fail("Невідома операція: " + operation);
                    break;
            }
        }

        // ------------------------------------------------------------- Then ---

        [Then(@"the booking is created and has a number")]
        public void ThenTheBookingIsCreatedAndHasANumber()
        {
            AssertStatus(HttpStatusCode.OK);

            var created = Deserialize<CreatedBooking>(_context.Response);

            Assert.IsNotNull(created, "Відповідь не є JSON. " + _booker.ExchangeLog);
            Assert.IsTrue(created.BookingId > 0,
                "Сервіс не повернув номера бронювання. " + _booker.ExchangeLog);
            Assert.IsNotNull(created.Booking,
                "У відповіді немає самого бронювання. " + _booker.ExchangeLog);

            AssertSameBooking(_context.SentBooking, created.Booking);
        }

        [Then(@"the booking matches the data I sent")]
        [Then(@"the booking matches the replacement data")]
        public void ThenTheBookingMatchesTheSentData()
        {
            AssertStatus(HttpStatusCode.OK);

            var actual = Deserialize<Booking>(_context.Response);

            Assert.IsNotNull(actual, "Відповідь не є JSON. " + _booker.ExchangeLog);
            AssertSameBooking(_context.SentBooking, actual);
        }

        [Then(@"the booking is gone")]
        public void ThenTheBookingIsGone()
        {
            RestResponse read = _booker.GetBooking(_context.BookingId);

            Assert.AreEqual(HttpStatusCode.NotFound, read.StatusCode,
                "Видалене бронювання все ще читається. " + _booker.ExchangeLog);
        }

        [Then(@"the list contains the created booking number")]
        public void ThenTheListContainsTheCreatedBookingNumber()
        {
            AssertStatus(HttpStatusCode.OK);

            var ids = Deserialize<List<BookingId>>(_context.Response);

            Assert.IsNotNull(ids, "Відповідь не є списком. " + _booker.ExchangeLog);
            Assert.IsTrue(ids.Count > 0, "Список бронювань порожній. " + _booker.ExchangeLog);
            Assert.IsTrue(ids.Any(item => item.Value == _context.BookingId),
                "У списку немає бронювання " + _context.BookingId + ". " + _booker.ExchangeLog);
        }

        [Then(@"the total price equals (\d+)")]
        public void ThenTheTotalPriceEquals(int expected)
        {
            AssertStatus(HttpStatusCode.OK);

            var actual = Deserialize<Booking>(_context.Response);

            Assert.IsNotNull(actual, "Відповідь не є JSON. " + _booker.ExchangeLog);
            Assert.AreEqual(expected, actual.TotalPrice,
                "Часткова зміна не записала нову суму. " + _booker.ExchangeLog);
        }

        [Then(@"the other fields keep the values I sent")]
        public void ThenTheOtherFieldsKeepTheirValues()
        {
            var actual = Deserialize<Booking>(_context.Response);

            Assert.IsNotNull(actual, "Відповідь не є JSON. " + _booker.ExchangeLog);
            AssertSameBooking(_context.SentBooking, actual);
        }

        // Кроки перевірки коду відповіді оголошені у CommonSteps: вони
        // однакові для обох служб.

        // -------------------------------------------------------- помічники ---

        /// <summary>
        /// Запамятовує номер створеного бронювання.
        /// </summary>
        /// <remarks>
        /// Викликається з кроку When, бо саме він створює ресурс. Кроки Then
        /// лише перевіряють результат і стану сценарію не змінюють, інакше
        /// сценарій, який не містить кроку перевірки створення, лишився б без
        /// номера бронювання і працював би з ресурсом за номером нуль.
        /// </remarks>
        private void RememberCreatedBooking()
        {
            var created = Deserialize<CreatedBooking>(_context.Response);

            // Неповне тіло запиту сервіс відкидає, номера в такому разі немає.
            if (created == null || created.BookingId <= 0)
                return;

            _context.BookingId = created.BookingId;
            _context.CreatedBookingIds.Add(created.BookingId);
        }

        private void AssertStatus(HttpStatusCode expected)
        {
            Assert.AreEqual(expected, _context.Response.StatusCode,
                "Код відповіді не той, що очікувався. " + _booker.ExchangeLog);
        }

        /// <summary>Порівнює бронювання за всіма полями.</summary>
        private static void AssertSameBooking(Booking expected, Booking actual)
        {
            Assert.AreEqual(expected.FirstName, actual.FirstName, "Не збігається імя клієнта.");
            Assert.AreEqual(expected.LastName, actual.LastName, "Не збігається прізвище клієнта.");
            Assert.AreEqual(expected.TotalPrice, actual.TotalPrice, "Не збігається сума.");
            Assert.AreEqual(expected.DepositPaid, actual.DepositPaid, "Не збігається ознака депозиту.");
            Assert.AreEqual(expected.AdditionalNeeds, actual.AdditionalNeeds, "Не збігаються побажання.");

            Assert.IsNotNull(actual.BookingDates, "У відповіді немає дат бронювання.");
            Assert.AreEqual(expected.BookingDates.CheckIn, actual.BookingDates.CheckIn,
                "Не збігається дата заїзду.");
            Assert.AreEqual(expected.BookingDates.CheckOut, actual.BookingDates.CheckOut,
                "Не збігається дата виїзду.");
        }

        private static T Deserialize<T>(RestResponse response) where T : class
        {
            if (string.IsNullOrWhiteSpace(response.Content))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (JsonException)
            {
                // Сервіс не зобовязаний повертати JSON у разі помилки: на
                // кодах 404 і 405 тіло є простим текстом.
                return null;
            }
        }
    }
}
