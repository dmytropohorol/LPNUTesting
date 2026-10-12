using System.Globalization;
using RestSharp;

namespace ApiTests.Clients
{
    /// <summary>
    /// Клієнт сервісу Open-Meteo, категорія Weather зі списку відкритих API.
    /// Ресурс лише читається, запису немає, тому маркер доступу не потрібен.
    /// </summary>
    public class WeatherClient : ApiClientBase
    {
        public const string BaseUrl = "https://api.open-meteo.com";

        /// <summary>Координати Львова. Стала точка відліку для сценаріїв.</summary>
        public const double LvivLatitude = 49.8397;
        public const double LvivLongitude = 24.0297;

        public WeatherClient() : base(BaseUrl)
        {
        }

        /// <summary>Погода на цю мить у заданій точці.</summary>
        public RestResponse GetCurrent(double latitude, double longitude)
        {
            var request = Forecast(latitude, longitude);
            request.AddQueryParameter("current", "temperature_2m,wind_speed_10m");
            return Send(request);
        }

        /// <summary>Погода на цю мить у заданих одиницях температури.</summary>
        public RestResponse GetCurrentInUnit(double latitude, double longitude, string temperatureUnit)
        {
            var request = Forecast(latitude, longitude);
            request.AddQueryParameter("current", "temperature_2m");
            request.AddQueryParameter("temperature_unit", temperatureUnit);
            return Send(request);
        }

        /// <summary>Прогноз по днях на задану кількість днів.</summary>
        public RestResponse GetDaily(double latitude, double longitude, int days)
        {
            var request = Forecast(latitude, longitude);
            request.AddQueryParameter("daily", "temperature_2m_max,temperature_2m_min");
            request.AddQueryParameter("forecast_days", days.ToString(CultureInfo.InvariantCulture));
            request.AddQueryParameter("timezone", "auto");
            return Send(request);
        }

        /// <summary>
        /// Запит лише з широтою. Потрібен для перевірки, що сервіс не приймає
        /// неповний набір обов'язкових параметрів.
        /// </summary>
        public RestResponse GetWithLatitudeOnly(double latitude)
        {
            var request = new RestRequest("v1/forecast", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddQueryParameter("latitude", Number(latitude));
            request.AddQueryParameter("current", "temperature_2m");
            return Send(request);
        }

        private static RestRequest Forecast(double latitude, double longitude)
        {
            var request = new RestRequest("v1/forecast", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddQueryParameter("latitude", Number(latitude));
            request.AddQueryParameter("longitude", Number(longitude));
            return request;
        }

        /// <summary>
        /// Число для рядка запиту. Культура задана явно: за української
        /// культури роздільником дробової частини є кома, і сервіс отримав би
        /// широту 49,8397 замість 49.8397.
        /// </summary>
        private static string Number(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
