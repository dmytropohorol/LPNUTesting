using System;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace ApiTests.Clients
{
    public abstract class ApiClientBase : IDisposable
    {
        private readonly RestClient _client;

        protected ApiClientBase(string baseUrl)
        {
            var options = new RestClientOptions(baseUrl)
            {
                Timeout = TimeSpan.FromSeconds(30),

                // Код відповіді перевіряє тест. Інакше RestSharp кидав би виняток
                // на кожному коді поза 2xx і негативні сценарії не мали б що перевіряти.
                ThrowOnAnyError = false,
                ThrowOnDeserializationError = false
            };

            _client = new RestClient(options,
                configureSerialization: s => s.UseNewtonsoftJson());
        }

        protected RestResponse Send(RestRequest request)
        {
            return _client.ExecuteAsync(request).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
