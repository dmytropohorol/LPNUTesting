using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BankingTests.Drivers
{
    /// <summary>
    /// Керує життєвим циклом браузера. Екземпляр створює SpecFlow через
    /// вбудований контейнер залежностей: один об'єкт на один сценарій.
    /// </summary>
    /// <remarks>
    /// Браузер відкривається лише під час першого звернення до
    /// <see cref="Current"/>, тому сценарії без роботи з UI його не запускають.
    /// Драйвер chromedriver.exe завантажувати вручну не потрібно: Selenium
    /// версії 4.6 і новішої має вбудований Selenium Manager, який підбирає
    /// драйвер під встановлену версію браузера.
    /// </remarks>
    public class BrowserDriver : IDisposable
    {
        /// <summary>Загальний час очікування появи елемента.</summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

        private readonly Lazy<IWebDriver> _driver;
        private bool _disposed;

        public BrowserDriver()
        {
            _driver = new Lazy<IWebDriver>(CreateDriver);
        }

        public IWebDriver Current
        {
            get { return _driver.Value; }
        }

        /// <summary>Нове явне очікування. Неявні очікування не використовуємо.</summary>
        public WebDriverWait CreateWait()
        {
            return CreateWait(DefaultTimeout);
        }

        public WebDriverWait CreateWait(TimeSpan timeout)
        {
            var wait = new WebDriverWait(Current, timeout);
            wait.PollingInterval = TimeSpan.FromMilliseconds(200);
            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException),
                typeof(ElementNotInteractableException));
            return wait;
        }

        private static IWebDriver CreateDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-infobars");

            // Запуск без вікна: HEADLESS=1 у змінних середовища.
            // Знадобиться, наприклад, для прогону на сервері складання.
            if (string.Equals(Environment.GetEnvironmentVariable("HEADLESS"), "1",
                              StringComparison.Ordinal))
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--window-size=1920,1080");
            }

            var driver = new ChromeDriver(options);

            // Неявне очікування навмисно вимкнене: воно змішується з явним
            // і робить час очікування непередбачуваним.
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

            return driver;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (!_driver.IsValueCreated)
                return;

            try
            {
                _driver.Value.Quit();
            }
            catch (WebDriverException)
            {
                // браузер міг уже закритися: для прибирання це не критично
            }
            finally
            {
                _driver.Value.Dispose();
            }
        }
    }
}
