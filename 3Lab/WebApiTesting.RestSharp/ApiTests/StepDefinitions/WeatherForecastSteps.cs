using System.Collections.Generic;
using ApiTests.Clients;
using ApiTests.Models;
using ApiTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace ApiTests.StepDefinitions
{
    /// <summary>
    /// Кроки сценаріїв прогнозу погоди з файлу WeatherForecast.feature.
    /// Друге завдання роботи: категорія Weather зі списку відкритих API.
    /// </summary>
    [Binding]
    public class WeatherForecastSteps
    {
        /// <summary>
        /// Допустима різниця координат. Сервіс округлює запитану точку до
        /// вузла своєї сітки, тому порівнювати координати на точну рівність
        /// не можна.
        /// </summary>
        private const double CoordinateTolerance = 0.5;

        private readonly WeatherClient _weather;
        private readonly ApiContext _context;

        private double _requestedLatitude;
        private double _requestedLongitude;

        public WeatherForecastSteps(WeatherClient weather, ApiContext context)
        {
            _weather = weather;
            _context = context;
        }

        // ------------------------------------------------------------ Given ---

        [Given(@"the weather service is used")]
        public void GivenTheWeatherServiceIsUsed()
        {
            // Служба не має ресурсу для перевірки доступності й не потребує
            // авторизації, тому крок лишає стан чистим. Він присутній, щоб
            // сценарії читалися як опис поведінки, а не як набір запитів.
            _context.Response = null;
        }

        // ------------------------------------------------------------- When ---

        [When(@"I ask for the current weather in Lviv")]
        public void WhenIAskForTheCurrentWeatherInLviv()
        {
            _requestedLatitude = WeatherClient.LvivLatitude;
            _requestedLongitude = WeatherClient.LvivLongitude;

            _context.Response = _weather.GetCurrent(_requestedLatitude, _requestedLongitude);
        }

        [When(@"I ask for the current weather at latitude (\S+) and longitude (\S+)")]
        public void WhenIAskForTheCurrentWeatherAt(double latitude, double longitude)
        {
            _requestedLatitude = latitude;
            _requestedLongitude = longitude;

            _context.Response = _weather.GetCurrent(latitude, longitude);
        }

        [When(@"I ask for the current weather in the unit ""(.*)""")]
        public void WhenIAskForTheCurrentWeatherInUnit(string unit)
        {
            _context.Response = _weather.GetCurrentInUnit(
                WeatherClient.LvivLatitude, WeatherClient.LvivLongitude, unit);
        }

        [When(@"I ask for the current weather with the latitude only")]
        public void WhenIAskForTheCurrentWeatherWithLatitudeOnly()
        {
            _context.Response = _weather.GetWithLatitudeOnly(WeatherClient.LvivLatitude);
        }

        [When(@"I ask for a daily forecast for (\d+) days")]
        public void WhenIAskForADailyForecast(int days)
        {
            _requestedLatitude = WeatherClient.LvivLatitude;
            _requestedLongitude = WeatherClient.LvivLongitude;

            _context.Response = _weather.GetDaily(_requestedLatitude, _requestedLongitude, days);
        }

        // ------------------------------------------------------------- Then ---

        [Then(@"the answer contains the current temperature")]
        public void ThenTheAnswerContainsTheCurrentTemperature()
        {
            Forecast forecast = Parse();

            Assert.IsNotNull(forecast.Current,
                "У відповіді немає блоку current. " + _weather.ExchangeLog);
            Assert.IsTrue(forecast.Current.ContainsKey("temperature_2m"),
                "У відповіді немає температури. " + _weather.ExchangeLog);
            Assert.IsNotNull(forecast.Current["temperature_2m"],
                "Температура порожня. " + _weather.ExchangeLog);

            Assert.IsNotNull(forecast.CurrentUnits,
                "У відповіді немає одиниць вимірювання. " + _weather.ExchangeLog);
            Assert.IsTrue(forecast.CurrentUnits.ContainsKey("temperature_2m"),
                "Не вказано одиниці температури. " + _weather.ExchangeLog);
        }

        [Then(@"the returned coordinates are close to the requested ones")]
        public void ThenTheReturnedCoordinatesAreClose()
        {
            Forecast forecast = Parse();

            Assert.AreEqual(_requestedLatitude, forecast.Latitude, CoordinateTolerance,
                "Широта у відповіді надто далеко від запитаної.");
            Assert.AreEqual(_requestedLongitude, forecast.Longitude, CoordinateTolerance,
                "Довгота у відповіді надто далеко від запитаної.");
        }

        [Then(@"the forecast contains exactly (\d+) dates")]
        public void ThenTheForecastContainsExactlyDates(int expected)
        {
            Forecast forecast = Parse();

            Assert.IsNotNull(forecast.Daily,
                "У відповіді немає блоку daily. " + _weather.ExchangeLog);
            Assert.IsTrue(forecast.Daily.ContainsKey("time"),
                "У прогнозі немає переліку дат. " + _weather.ExchangeLog);

            List<object> dates = forecast.Daily["time"];

            Assert.AreEqual(expected, dates.Count,
                "Кількість днів у прогнозі не та, що запитувалася. " + _weather.ExchangeLog);
        }

        [Then(@"the time zone of the answer is ""(.*)""")]
        public void ThenTheTimeZoneIs(string expected)
        {
            Forecast forecast = Parse();

            Assert.AreEqual(expected, forecast.Timezone,
                "Часовий пояс не той, що очікувався. " + _weather.ExchangeLog);
        }

        [Then(@"the unit of the temperature is ""(.*)""")]
        public void ThenTheUnitOfTheTemperatureIs(string expected)
        {
            Forecast forecast = Parse();

            Assert.IsNotNull(forecast.CurrentUnits,
                "У відповіді немає одиниць вимірювання. " + _weather.ExchangeLog);
            Assert.AreEqual(expected, forecast.CurrentUnits["temperature_2m"],
                "Одиниця температури не та, що запитувалася. " + _weather.ExchangeLog);
        }

        [Then(@"the refusal reason mentions ""(.*)""")]
        public void ThenTheRefusalReasonMentions(string fragment)
        {
            var error = JsonConvert.DeserializeObject<ApiError>(_context.Response.Content);

            Assert.IsNotNull(error, "Відповідь не є JSON. " + _weather.ExchangeLog);
            Assert.IsTrue(error.Error,
                "У відповіді не піднята ознака помилки. " + _weather.ExchangeLog);
            Assert.IsFalse(string.IsNullOrEmpty(error.Reason),
                "Сервіс не пояснив причини відмови. " + _weather.ExchangeLog);

            StringAssert.Contains(error.Reason, fragment,
                "Причина відмови не згадує " + fragment + ". " + _weather.ExchangeLog);
        }

        // -------------------------------------------------------- помічники ---

        /// <summary>Розбирає останню відповідь у модель прогнозу.</summary>
        private Forecast Parse()
        {
            Assert.IsNotNull(_context.Response, "Сценарій не надіслав жодного запиту.");

            var forecast = JsonConvert.DeserializeObject<Forecast>(_context.Response.Content);

            Assert.IsNotNull(forecast,
                "Не вдалося розібрати відповідь. " + _weather.ExchangeLog);

            return forecast;
        }
    }
}
