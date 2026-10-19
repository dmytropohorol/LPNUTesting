using BankingTests.Drivers;
using TechTalk.SpecFlow;

namespace BankingTests.Hooks
{
    [Binding]
    public class ScenarioHooks
    {
        private readonly BrowserDriver _browser;

        public ScenarioHooks(BrowserDriver browser)
        {
            _browser = browser;
        }

        [AfterScenario]
        public void CloseBrowser()
        {
            _browser.Dispose();
        }
    }
}
