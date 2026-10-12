using ApiTests.Clients;
using ApiTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace ApiTests.StepDefinitions
{
    /// <summary>
    /// Кроки, спільні для всіх сценаріїв: перевірка коду відповіді.
    /// </summary>
    /// <remarks>
    /// Такий крок потрібен однаково обом службам, тому оголошений один раз.
    /// Якби він повторювався у кожному файлі кроків, SpecFlow під час прогону
    /// повідомив би про неоднозначне визначення кроку.
    /// </remarks>
    [Binding]
    public class CommonSteps
    {
        private readonly ApiContext _context;
        private readonly BookerClient _booker;
        private readonly WeatherClient _weather;

        public CommonSteps(ApiContext context, BookerClient booker, WeatherClient weather)
        {
            _context = context;
            _booker = booker;
            _weather = weather;
        }

        [Then(@"the response status is (\d+)")]
        public void ThenTheResponseStatusIs(int expected)
        {
            Assert.IsNotNull(_context.Response, "Сценарій не надіслав жодного запиту.");

            Assert.AreEqual(expected, (int)_context.Response.StatusCode,
                "Код відповіді не той, що очікувався. " + Diagnostics);
        }

        [Then(@"the request is rejected")]
        public void ThenTheRequestIsRejected()
        {
            Assert.IsNotNull(_context.Response, "Сценарій не надіслав жодного запиту.");

            int code = (int)_context.Response.StatusCode;

            // Перевіряється сам факт відмови, а не конкретний код: сервіс
            // restful-booker відповідає кодами 500 і 405 там, де за
            // специфікацією HTTP належали б 400 і 404. Перелік відхилень
            // наведений у звіті.
            Assert.IsTrue(code >= 400,
                "Некоректний запит виконався успішно, код " + code + ". " + Diagnostics);
        }

        /// <summary>
        /// Журнал обміну для повідомлення про падіння. Жоден сценарій не
        /// звертається до двох служб одночасно, тому журнал клієнта, якого
        /// сценарій не зачіпав, порожній, і обидва можна просто склеїти.
        /// </summary>
        private string Diagnostics
        {
            get { return _booker.ExchangeLog + _weather.ExchangeLog; }
        }
    }
}
