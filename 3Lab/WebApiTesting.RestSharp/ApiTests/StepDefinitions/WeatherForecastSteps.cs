using ApiTests.Clients;
using ApiTests.Models;
using ApiTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace ApiTests.StepDefinitions
{
    [Binding]
    public class WeatherForecastSteps
    {
        // Сервіс зсуває точку до вузла своєї сітки, тому координати не збігаються точно.
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

        [Given(@"the weather service is used")]
        public void GivenTheWeatherServiceIsUsed()
        {
            _context.Response = null;
        }

        [When(@"I ask for the current weather in Lviv")]
        public void WhenIAskForTheCurrentWeatherInLviv()
        {
            WhenIAskForTheCurrentWeatherAt(WeatherClient.LvivLatitude, WeatherClient.LvivLongitude);
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
            _context.Response = _weather.GetDaily(WeatherClient.LvivLatitude, WeatherClient.LvivLongitude, days);
        }

        [Then(@"the answer contains the current temperature")]
        public void ThenTheAnswerContainsTheCurrentTemperature()
        {
            Forecast forecast = Parse();

            Assert.IsNotNull(forecast.Current, "У відповіді немає блоку current.");
            Assert.IsNotNull(forecast.Current["temperature_2m"], "У відповіді немає температури.");
            Assert.IsTrue(forecast.CurrentUnits.ContainsKey("temperature_2m"), "Не вказано одиниці температури.");
        }

        [Then(@"the returned coordinates are close to the requested ones")]
        public void ThenTheReturnedCoordinatesAreClose()
        {
            Forecast forecast = Parse();

            Assert.AreEqual(_requestedLatitude, forecast.Latitude, CoordinateTolerance, "Широта надто далеко від запитаної.");
            Assert.AreEqual(_requestedLongitude, forecast.Longitude, CoordinateTolerance, "Довгота надто далеко від запитаної.");
        }

        [Then(@"the forecast contains exactly (\d+) dates")]
        public void ThenTheForecastContainsExactlyDates(int expected)
        {
            Forecast forecast = Parse();

            Assert.IsNotNull(forecast.Daily, "У відповіді немає блоку daily.");
            Assert.AreEqual(expected, forecast.Daily["time"].Count, "Кількість днів не та, що запитувалася.");
        }

        [Then(@"the time zone of the answer is ""(.*)""")]
        public void ThenTheTimeZoneIs(string expected)
        {
            Assert.AreEqual(expected, Parse().Timezone, "Часовий пояс не той, що очікувався.");
        }

        [Then(@"the unit of the temperature is ""(.*)""")]
        public void ThenTheUnitOfTheTemperatureIs(string expected)
        {
            Assert.AreEqual(expected, Parse().CurrentUnits["temperature_2m"], "Одиниця температури не та, що запитувалася.");
        }

        [Then(@"the refusal reason mentions ""(.*)""")]
        public void ThenTheRefusalReasonMentions(string fragment)
        {
            var error = JsonConvert.DeserializeObject<ApiError>(_context.Response.Content);

            Assert.IsNotNull(error, "Відповідь не є JSON.");
            Assert.IsTrue(error.Error, "У відповіді не піднята ознака помилки.");
            StringAssert.Contains(error.Reason, fragment, "Причина відмови не згадує " + fragment + ".");
        }

        private Forecast Parse()
        {
            var forecast = JsonConvert.DeserializeObject<Forecast>(_context.Response.Content);
            Assert.IsNotNull(forecast, "Не вдалося розібрати відповідь: " + _context.Response.Content);
            return forecast;
        }
    }
}
