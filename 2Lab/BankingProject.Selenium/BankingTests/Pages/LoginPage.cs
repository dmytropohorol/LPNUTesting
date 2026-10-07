using BankingTests.Drivers;
using OpenQA.Selenium;

namespace BankingTests.Pages
{
    /// <summary>
    /// Стартова сторінка застосунку з вибором ролі: клієнт або менеджер банку.
    /// </summary>
    public class LoginPage : BasePage
    {
        // Локатори беремо за атрибутом ng-click: у цьому застосунку він стабільніший
        // за текст кнопки та за класи оформлення Bootstrap.
        private static readonly By CustomerLoginButton = By.CssSelector("button[ng-click='customer()']");
        private static readonly By ManagerLoginButton = By.CssSelector("button[ng-click='manager()']");
        private static readonly By HomeButton = By.CssSelector("button.home");

        public LoginPage(BrowserDriver browser) : base(browser)
        {
        }

        /// <summary>Відкриває застосунок і чекає готовності стартової сторінки.</summary>
        public void Open()
        {
            Driver.Navigate().GoToUrl(BaseUrl);
            Visible(ManagerLoginButton);
        }

        public bool IsOpen
        {
            get { return IsVisible(ManagerLoginButton, BrowserDriver.DefaultTimeout); }
        }

        /// <summary>Вхід у режим менеджера банку.</summary>
        public void LoginAsBankManager()
        {
            Click(ManagerLoginButton);
            WaitForUrlFragment("#/manager");
        }

        public void LoginAsCustomer()
        {
            Click(CustomerLoginButton);
            WaitForUrlFragment("#/customer");
        }

        public void GoHome()
        {
            Click(HomeButton);
        }
    }
}
