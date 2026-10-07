using System;
using System.Linq;
using BankingTests.Drivers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace BankingTests.Pages
{
    /// <summary>
    /// Спільна основа для сторінок (Page Objects Pattern).
    /// Тут зібрані дії з елементами, щоб сторінки містили лише локатори
    /// та операції предметної області, а кроки тестів не знали про Selenium.
    /// </summary>
    public abstract class BasePage
    {
        /// <summary>Адреса застосунку XYZ Bank.</summary>
        public const string BaseUrl =
            "https://www.globalsqa.com/angularJs-protractor/BankingProject/";

        protected readonly BrowserDriver Browser;

        protected BasePage(BrowserDriver browser)
        {
            Browser = browser;
        }

        protected IWebDriver Driver
        {
            get { return Browser.Current; }
        }

        /// <summary>Чекає, доки елемент з'явиться в DOM і стане видимим.</summary>
        protected IWebElement Visible(By locator)
        {
            return Browser.CreateWait().Until(driver =>
            {
                var element = driver.FindElements(locator).FirstOrDefault();
                return element != null && element.Displayed ? element : null;
            });
        }

        /// <summary>Чи присутній видимий елемент протягом заданого часу.</summary>
        protected bool IsVisible(By locator, TimeSpan timeout)
        {
            try
            {
                Browser.CreateWait(timeout).Until(driver =>
                {
                    var element = driver.FindElements(locator).FirstOrDefault();
                    return element != null && element.Displayed ? element : null;
                });
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        protected void Click(By locator)
        {
            Visible(locator).Click();
        }

        protected SelectElement Dropdown(By locator)
        {
            return new SelectElement(Visible(locator));
        }

        /// <summary>Чекає, доки адреса сторінки міститиме заданий фрагмент.</summary>
        protected void WaitForUrlFragment(string fragment)
        {
            Browser.CreateWait().Until(driver =>
                driver.Url.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public string CurrentUrl
        {
            get { return Driver.Url; }
        }
    }
}
