using System;
using ApiTests.Clients;
using ApiTests.Support;
using RestSharp;
using TechTalk.SpecFlow;

namespace ApiTests.Hooks
{
    /// <summary>
    /// Дії навколо сценаріїв: прибирання створених бронювань та журнал обміну
    /// у разі падіння.
    /// </summary>
    [Binding]
    public class ApiHooks
    {
        private readonly BookerClient _booker;
        private readonly WeatherClient _weather;
        private readonly ApiContext _context;
        private readonly ScenarioContext _scenarioContext;

        public ApiHooks(BookerClient booker,
                        WeatherClient weather,
                        ApiContext context,
                        ScenarioContext scenarioContext)
        {
            _booker = booker;
            _weather = weather;
            _context = context;
            _scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Видаляє бронювання, які створив сценарій. Сервіс restful-booker
        /// спільний для всіх, хто його використовує, тому лишати по собі
        /// записи не варто.
        /// </summary>
        /// <remarks>
        /// Прибирання йде першим за порядком, бо наступний хук друкує журнал
        /// обміну і запити на видалення мають потрапити до нього.
        /// Помилка прибирання не повинна валити сценарій, який уже пройшов,
        /// тому виняток лише згадується у виводі.
        /// </remarks>
        [AfterScenario(Order = 100)]
        public void RemoveCreatedBookings()
        {
            if (_context.CreatedBookingIds.Count == 0)
                return;

            string token = _context.Token;

            if (string.IsNullOrEmpty(token))
            {
                // Сценарій міг не брати маркера: створення його не вимагає.
                // Для видалення маркер обовязковий, тож беремо його тут.
                token = TryGetToken();

                if (token == null)
                {
                    Console.WriteLine("Прибирання пропущено: не вдалося отримати маркер доступу.");
                    return;
                }
            }

            foreach (int id in _context.CreatedBookingIds)
            {
                try
                {
                    _booker.DeleteBooking(id, token);
                }
                catch (Exception error)
                {
                    Console.WriteLine("Не вдалося видалити бронювання " + id + ": " + error.Message);
                }
            }

            _context.CreatedBookingIds.Clear();
        }

        /// <summary>
        /// Друкує журнал обміну, коли сценарій упав. У звіті прогону лишається
        /// лише назва кроку, а для розбору потрібні самі запити та відповіді.
        /// </summary>
        [AfterScenario(Order = 200)]
        public void PrintExchangeLogOnFailure()
        {
            if (_scenarioContext.TestError == null)
                return;

            Console.WriteLine("Обмін із сервісом у сценарії \""
                + _scenarioContext.ScenarioInfo.Title + "\":");
            Console.WriteLine(_booker.ExchangeLog + _weather.ExchangeLog);
        }

        /// <summary>
        /// Закриває клієнтів. RestSharp тримає HttpClient, тож його треба
        /// звільнити разом зі сценарієм.
        /// </summary>
        [AfterScenario(Order = 300)]
        public void CloseClients()
        {
            _booker.Dispose();
            _weather.Dispose();
        }

        private string TryGetToken()
        {
            try
            {
                RestResponse response = _booker.CreateToken(
                    BookerClient.AdminUser, BookerClient.AdminPassword);

                var auth = Newtonsoft.Json.JsonConvert
                    .DeserializeObject<Models.AuthResult>(response.Content);

                return auth == null || string.IsNullOrEmpty(auth.Token) ? null : auth.Token;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
