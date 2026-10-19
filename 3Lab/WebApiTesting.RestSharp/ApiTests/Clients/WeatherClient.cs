using System.Globalization;
using RestSharp;

namespace ApiTests.Clients
{
    public class WeatherClient : ApiClientBase
    {
        public const string BaseUrl = "https://api.open-meteo.com";
        public const double LvivLatitude = 49.8397;
        public const double LvivLongitude = 24.0297;

        public WeatherClient() : base(BaseUrl)
        {
        }

        public RestResponse GetCurrent(double latitude, double longitude)
        {
            var request = Forecast(latitude, longitude);
            request.AddQueryParameter("current", "temperature_2m,wind_speed_10m");
            return Send(request);
        }

        public RestResponse GetCurrentInUnit(double latitude, double longitude, string temperatureUnit)
        {
            var request = Forecast(latitude, longitude);
            request.AddQueryParameter("current", "temperature_2m");
            request.AddQueryParameter("temperature_unit", temperatureUnit);
            return Send(request);
        }

        public RestResponse GetDaily(double latitude, double longitude, int days)
        {
            var request = Forecast(latitude, longitude);
            request.AddQueryParameter("daily", "temperature_2m_max,temperature_2m_min");
            request.AddQueryParameter("forecast_days", days.ToString(CultureInfo.InvariantCulture));
            request.AddQueryParameter("timezone", "auto");
            return Send(request);
        }

        public RestResponse GetWithLatitudeOnly(double latitude)
        {
            var request = new RestRequest("v1/forecast", Method.Get);
            request.AddQueryParameter("latitude", Number(latitude));
            request.AddQueryParameter("current", "temperature_2m");
            return Send(request);
        }

        private static RestRequest Forecast(double latitude, double longitude)
        {
            var request = new RestRequest("v1/forecast", Method.Get);
            request.AddQueryParameter("latitude", Number(latitude));
            request.AddQueryParameter("longitude", Number(longitude));
            return request;
        }

        // За української культури роздільник дробової частини кома,
        // і сервіс отримав би 49,8397 замість 49.8397.
        private static string Number(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
