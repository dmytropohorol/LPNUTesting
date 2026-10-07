using System;
using System.Linq;
using BankingTests.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace BankingTests.StepDefinitions
{
    /// <summary>
    /// Реалізація кроків сценаріїв з файлу OpenAccount.feature.
    /// Кроки не працюють із Selenium напряму: усі дії делеговані сторінкам
    /// (Page Objects Pattern), тому зміна верстки сайту не зачіпає цей файл.
    /// </summary>
    [Binding]
    public class OpenAccountSteps
    {
        /// <summary>Скільки чекати на діалог alert, коли він очікується.</summary>
        private static readonly TimeSpan AlertTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Скільки чекати, аби переконатися, що діалогу немає. Час менший:
        /// цей крок завжди витрачає його повністю.
        /// </summary>
        private static readonly TimeSpan NoAlertTimeout = TimeSpan.FromSeconds(3);

        private readonly LoginPage _loginPage;
        private readonly ManagerPage _managerPage;
        private readonly OpenAccountPage _openAccountPage;

        private string _alertText;

        // Сторінки створює контейнер SpecFlow: по одному екземпляру на сценарій.
        public OpenAccountSteps(LoginPage loginPage,
                                ManagerPage managerPage,
                                OpenAccountPage openAccountPage)
        {
            _loginPage = loginPage;
            _managerPage = managerPage;
            _openAccountPage = openAccountPage;
        }

        // ------------------------------------------------------------ Given ---

        [Given(@"the XYZ Bank application is open")]
        public void GivenTheApplicationIsOpen()
        {
            _loginPage.Open();
            Assert.IsTrue(_loginPage.IsOpen, "Стартова сторінка XYZ Bank не відкрилася.");
        }

        [Given(@"I am logged in as Bank Manager")]
        public void GivenIAmLoggedInAsBankManager()
        {
            _loginPage.LoginAsBankManager();
            Assert.IsTrue(_managerPage.IsOpen, "Сторінка менеджера банку не відкрилася.");
        }

        // ------------------------------------------------------------- When ---

        [When(@"I open the Open Account tab")]
        public void WhenIOpenTheOpenAccountTab()
        {
            _managerPage.GoToOpenAccount();
            Assert.IsTrue(_openAccountPage.IsOpen, "Вкладка Open Account не відкрилася.");
        }

        [When(@"I select customer ""(.*)""")]
        public void WhenISelectCustomer(string customerName)
        {
            _openAccountPage.SelectCustomer(customerName);
        }

        [When(@"I select currency ""(.*)""")]
        public void WhenISelectCurrency(string currency)
        {
            _openAccountPage.SelectCurrency(currency);
        }

        [When(@"I process the account opening")]
        public void WhenIProcessTheAccountOpening()
        {
            _openAccountPage.Process();
        }

        // ------------------------------------------------------------- Then ---

        [Then(@"the account creation message is shown")]
        public void ThenTheAccountCreationMessageIsShown()
        {
            _alertText = _openAccountPage.ReadAndAcceptAlert(AlertTimeout);

            Assert.IsNotNull(_alertText,
                "Очікувалося повідомлення про створення рахунку, але діалог не з'явився.");

            StringAssert.Contains(_alertText, "Account created successfully",
                "Текст повідомлення не відповідає очікуваному.");
        }

        [Then(@"the created account number is not empty")]
        public void ThenTheCreatedAccountNumberIsNotEmpty()
        {
            string accountNumber = OpenAccountPage.ExtractAccountNumber(_alertText);

            Assert.IsFalse(string.IsNullOrWhiteSpace(accountNumber),
                "У повідомленні немає номера рахунку: " + _alertText);

            Assert.IsTrue(accountNumber.All(char.IsDigit),
                "Номер рахунку має складатися лише з цифр, отримано: " + accountNumber);
        }

        [Then(@"no account creation message is shown")]
        public void ThenNoAccountCreationMessageIsShown()
        {
            string text = _openAccountPage.ReadAndAcceptAlert(NoAlertTimeout);

            Assert.IsNull(text,
                "Рахунок не мав створитися, але застосунок показав повідомлення: " + text);
        }

        [Then(@"I stay on the Open Account page")]
        public void ThenIStayOnTheOpenAccountPage()
        {
            StringAssert.Contains(_openAccountPage.CurrentUrl, "#/manager/openAccount",
                "Застосунок пішов з вкладки Open Account.");
        }

        [Then(@"the customer list contains:")]
        public void ThenTheCustomerListContains(Table table)
        {
            var expected = table.Rows.Select(row => row["customer"]).ToList();
            var actual = _openAccountPage.AvailableCustomers;

            foreach (string customer in expected)
            {
                CollectionAssert.Contains((System.Collections.ICollection)actual, customer,
                    "У списку клієнтів немає запису: " + customer);
            }
        }
    }
}
