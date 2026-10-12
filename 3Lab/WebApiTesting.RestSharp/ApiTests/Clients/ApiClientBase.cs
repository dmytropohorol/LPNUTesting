using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace ApiTests.Clients
{
    /// <summary>
    /// Спільна основа клієнтів Web API. Відповідає за налаштування RestSharp,
    /// надсилання запиту та журнал обміну.
    /// </summary>
    /// <remarks>
    /// Роль та сама, що в Page Object для Web тестування: кроки сценарію не
    /// знають ні про адреси ресурсів, ні про формат тіла запиту. Зміна
    /// контракту API зачіпає лише клієнта.
    /// </remarks>
    public abstract class ApiClientBase : IDisposable
    {
        /// <summary>Скільки чекати на відповідь сервера.</summary>
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        /// <summary>Скільки разів надсилати запит, поки сервіс не відповість.</summary>
        private const int MaxAttempts = 3;

        /// <summary>Пауза перед повторною спробою.</summary>
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

        private readonly RestClient _client;
        private readonly StringBuilder _log = new StringBuilder();
        private bool _disposed;

        protected ApiClientBase(string baseUrl)
        {
            var options = new RestClientOptions(baseUrl)
            {
                Timeout = Timeout,

                // Статус-код відповіді перевіряє тест. Якби бібліотека сама
                // кидала виняток на кожному коді поза 2xx, негативні сценарії
                // не змогли б перевірити ні код, ні тіло відповіді.
                ThrowOnAnyError = false,

                // Сервіс не зобов'язаний повертати коректний JSON у разі
                // помилки: restful-booker на 404 віддає простий текст.
                // Невдала десеріалізація не має зривати тест.
                ThrowOnDeserializationError = false
            };

            _client = new RestClient(options,
                configureSerialization: s => s.UseNewtonsoftJson());
        }

        /// <summary>Журнал обміну за сценарій. Потрібен для розбору падінь.</summary>
        public string ExchangeLog
        {
            get { return _log.ToString(); }
        }

        /// <summary>
        /// Надсилає запит і записує обмін у журнал. Повторює спробу, якщо
        /// сервіс не відповів або відповів збоєм на своєму боці.
        /// </summary>
        protected RestResponse Send(RestRequest request)
        {
            // Повторювати безпечно ті методи, для яких повторний виклик дає
            // той самий результат, що й один виклик: читання, повна заміна та
            // видалення. POST сюди не входить, бо другий такий запит створив
            // би ще один ресурс.
            bool safeToRepeat = request.Method == Method.Get
                                || request.Method == Method.Put
                                || request.Method == Method.Delete;

            return Send(request, safeToRepeat);
        }

        /// <summary>
        /// Надсилає запит, дозволяючи або забороняючи повтор явно. Потрібне
        /// для запитів, безпечність повтору яких визначається не методом HTTP.
        /// </summary>
        protected RestResponse Send(RestRequest request, bool safeToRepeat)
        {
            RestResponse response = null;

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                response = _client
                    .ExecuteAsync(request)
                    .GetAwaiter()
                    .GetResult();

                Record(request, response);

                if (attempt == MaxAttempts || !safeToRepeat || !IsTransientFailure(response))
                    break;

                _log.AppendLine("    спроба " + attempt + " не вдалася, повтор");
                Thread.Sleep(RetryDelay);
            }

            return response;
        }

        /// <summary>
        /// Чи є збій тимчасовим. Обидві служби зовнішні та спільні, тому
        /// поодинокий обрив зв'язку або код 5xx не означає помилки у тому,
        /// що перевіряється.
        /// </summary>
        /// <remarks>
        /// Коди 4xx тимчасовими не вважаються: для негативних сценаріїв це
        /// законний очікуваний результат, а не збій, і повторювати такий
        /// запит немає сенсу. Перевіряти тут ResponseStatus не можна:
        /// RestSharp позначає ним і справжні збої, і будь-яку відповідь із
        /// кодом поза 2xx, тому негативні сценарії марно повторювалися б.
        /// </remarks>
        private static bool IsTransientFailure(RestResponse response)
        {
            int code = (int)response.StatusCode;

            // Сервер не відповів зовсім: обрив зв'язку або вичерпаний час
            // очікування. Коду відповіді у такому разі немає.
            if (code == 0)
                return true;

            // Збій на боці сервера.
            return code >= 500;
        }

        /// <summary>
        /// Тіло відповіді як JObject. Дає перевіряти наявність та типи полів,
        /// не оголошуючи клас під кожну форму відповіді.
        /// </summary>
        protected static JObject AsJson(RestResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.Content))
                return null;

            try
            {
                return JObject.Parse(response.Content);
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                // Відповідь не JSON: на частині кодів сервіс віддає простий
                // текст. Для тесту це законний результат, а не збій.
                return null;
            }
        }

        private void Record(RestRequest request, RestResponse response)
        {
            _log.AppendFormat("{0} {1} -> {2} ({3})",
                request.Method,
                response.ResponseUri == null ? request.Resource : response.ResponseUri.ToString(),
                (int)response.StatusCode,
                response.StatusCode);
            _log.AppendLine();

            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                string body = response.Content.Length > 500
                    ? response.Content.Substring(0, 500) + "..."
                    : response.Content;
                _log.AppendLine("    " + body);
            }

            if (response.ErrorMessage != null)
                _log.AppendLine("    помилка передачі: " + response.ErrorMessage);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _client.Dispose();
        }
    }
}
