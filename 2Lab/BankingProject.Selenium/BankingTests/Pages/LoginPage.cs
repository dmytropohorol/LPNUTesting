using BankingTests.Drivers;
using OpenQA.Selenium;

namespace BankingTests.Pages
{
    public class LoginPage : BasePage
    {
        // Застосунок на AngularJS: ng-click стабільніший за текст кнопки і класи Bootstrap.
        private static readonly By ManagerLoginButton = By.CssSelector("button[ng-click='manager()']");

        public LoginPage(BrowserDriver browser) : base(browser)
        {
        }

        public void Open()
        {
            Driver.Navigate().GoToUrl(BaseUrl);
            Visible(ManagerLoginButton);
        }

        public bool IsOpen
        {
            get { return IsVisible(ManagerLoginButton, BrowserDriver.DefaultTimeout); }
        }

        public void LoginAsBankManager()
        {
            Click(ManagerLoginButton);
            WaitForUrlFragment("#/manager");
        }
    }
}
