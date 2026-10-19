using System;
using System.Linq;
using BankingTests.Drivers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace BankingTests.Pages
{
    public abstract class BasePage
    {
        public const string BaseUrl = "https://www.globalsqa.com/angularJs-protractor/BankingProject/";

        protected readonly BrowserDriver Browser;

        protected BasePage(BrowserDriver browser)
        {
            Browser = browser;
        }

        protected IWebDriver Driver
        {
            get { return Browser.Current; }
        }

        public string CurrentUrl
        {
            get { return Driver.Url; }
        }

        protected IWebElement Visible(By locator)
        {
            return Browser.CreateWait().Until(driver => FindVisible(driver, locator));
        }

        protected bool IsVisible(By locator, TimeSpan timeout)
        {
            try
            {
                Browser.CreateWait(timeout).Until(driver => FindVisible(driver, locator));
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

        protected void WaitForUrlFragment(string fragment)
        {
            Browser.CreateWait().Until(driver =>
                driver.Url.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static IWebElement FindVisible(IWebDriver driver, By locator)
        {
            var element = driver.FindElements(locator).FirstOrDefault();
            return element != null && element.Displayed ? element : null;
        }
    }
}
