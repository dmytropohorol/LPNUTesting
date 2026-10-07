using System;
using System.IO;
using BankingTests.Drivers;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace BankingTests.Hooks
{
    /// <summary>
    /// Дії навколо сценаріїв: знімок екрана у разі падіння та закриття браузера.
    /// </summary>
    [Binding]
    public class ScenarioHooks
    {
        private readonly BrowserDriver _browser;
        private readonly ScenarioContext _scenarioContext;

        public ScenarioHooks(BrowserDriver browser, ScenarioContext scenarioContext)
        {
            _browser = browser;
            _scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Знімок екрана допомагає розібратися, на чому саме впав сценарій:
        /// у звіті прогону лишається сама лише назва кроку.
        /// </summary>
        [AfterScenario(Order = 100)]
        public void CaptureScreenshotOnFailure()
        {
            if (_scenarioContext.TestError == null)
                return;

            try
            {
                var screenshotTaker = _browser.Current as ITakesScreenshot;
                if (screenshotTaker == null)
                    return;

                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                Directory.CreateDirectory(folder);

                string safeTitle = string.Join("_",
                    _scenarioContext.ScenarioInfo.Title.Split(Path.GetInvalidFileNameChars()));

                string file = Path.Combine(folder, string.Format("{0}_{1:yyyyMMdd_HHmmss}.png",
                    safeTitle, DateTime.Now));

                screenshotTaker.GetScreenshot().SaveAsFile(file);
                Console.WriteLine("Знімок екрана: " + file);
            }
            catch (WebDriverException)
            {
                // браузер міг уже завершитися: діагностику не вважаємо критичною
            }
        }

        /// <summary>
        /// Закриває браузер після сценарію. Порядок більший за знімок екрана,
        /// щоб знімок устиг зробитися до закриття вікна.
        /// </summary>
        [AfterScenario(Order = 200)]
        public void CloseBrowser()
        {
            _browser.Dispose();
        }
    }
}
