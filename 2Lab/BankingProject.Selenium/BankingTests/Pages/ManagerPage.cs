using BankingTests.Drivers;
using OpenQA.Selenium;

namespace BankingTests.Pages
{
    /// <summary>
    /// Сторінка менеджера банку з трьома вкладками:
    /// Add Customer, Open Account та Customers.
    /// </summary>
    public class ManagerPage : BasePage
    {
        private static readonly By AddCustomerTab = By.CssSelector("button[ng-click='addCust()']");
        private static readonly By OpenAccountTab = By.CssSelector("button[ng-click='openAccount()']");
        private static readonly By CustomersTab = By.CssSelector("button[ng-click='showCust()']");

        public ManagerPage(BrowserDriver browser) : base(browser)
        {
        }

        public bool IsOpen
        {
            get { return IsVisible(OpenAccountTab, BrowserDriver.DefaultTimeout); }
        }

        /// <summary>Переходить на вкладку відкриття рахунку.</summary>
        public void GoToOpenAccount()
        {
            Click(OpenAccountTab);
            WaitForUrlFragment("#/manager/openAccount");
        }

        public void GoToAddCustomer()
        {
            Click(AddCustomerTab);
            WaitForUrlFragment("#/manager/addCust");
        }

        public void GoToCustomers()
        {
            Click(CustomersTab);
            WaitForUrlFragment("#/manager/list");
        }
    }
}
