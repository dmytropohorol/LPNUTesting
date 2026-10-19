using BankingTests.Drivers;
using OpenQA.Selenium;

namespace BankingTests.Pages
{
    public class ManagerPage : BasePage
    {
        private static readonly By OpenAccountTab = By.CssSelector("button[ng-click='openAccount()']");

        public ManagerPage(BrowserDriver browser) : base(browser)
        {
        }

        public bool IsOpen
        {
            get { return IsVisible(OpenAccountTab, BrowserDriver.DefaultTimeout); }
        }

        public void GoToOpenAccount()
        {
            Click(OpenAccountTab);
            WaitForUrlFragment("#/manager/openAccount");
        }
    }
}
