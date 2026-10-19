using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BankingTests.Drivers
{
    // Один браузер на сценарій. chromedriver качати не треба:
    // Selenium 4.6+ сам підбирає його через Selenium Manager.
    public class BrowserDriver : IDisposable
    {
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

        public BrowserDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");

            Current = new ChromeDriver(options);

            // Лише явні очікування: змішані з неявними дають непередбачуваний час очікування.
            Current.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        }

        public IWebDriver Current { get; private set; }

        public WebDriverWait CreateWait()
        {
            return CreateWait(DefaultTimeout);
        }

        public WebDriverWait CreateWait(TimeSpan timeout)
        {
            var wait = new WebDriverWait(Current, timeout);
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException),
                typeof(ElementNotInteractableException));
            return wait;
        }

        public void Dispose()
        {
            Current.Quit();
        }
    }
}
