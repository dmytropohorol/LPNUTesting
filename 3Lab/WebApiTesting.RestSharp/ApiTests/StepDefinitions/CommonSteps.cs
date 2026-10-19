using ApiTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace ApiTests.StepDefinitions
{
    // Кроки, спільні для обох сервісів. Оголошені один раз: повтор в іншому
    // класі SpecFlow сприйняв би як неоднозначне визначення кроку.
    [Binding]
    public class CommonSteps
    {
        private readonly ApiContext _context;

        public CommonSteps(ApiContext context)
        {
            _context = context;
        }

        [Then(@"the response status is (\d+)")]
        public void ThenTheResponseStatusIs(int expected)
        {
            Assert.AreEqual(expected, (int)_context.Response.StatusCode, _context.Response.Content);
        }

        // restful-booker відповідає 500 і 405 там, де мали б бути 400 і 404,
        // тому перевіряється сам факт відмови.
        [Then(@"the request is rejected")]
        public void ThenTheRequestIsRejected()
        {
            Assert.IsTrue((int)_context.Response.StatusCode >= 400,
                "Некоректний запит виконався успішно, код " + (int)_context.Response.StatusCode);
        }
    }
}
