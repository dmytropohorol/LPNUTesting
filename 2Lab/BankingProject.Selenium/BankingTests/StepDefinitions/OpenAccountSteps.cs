using System;
using System.Linq;
using BankingTests.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace BankingTests.StepDefinitions
{
    // Кроки не працюють із Selenium напряму, лише з методами сторінок (Page Objects).
    [Binding]
    public class OpenAccountSteps
    {
        private static readonly TimeSpan AlertTimeout = TimeSpan.FromSeconds(10);

        // Коротше: коли діалогу не має бути, крок завжди чекає цей час повністю.
        private static readonly TimeSpan NoAlertTimeout = TimeSpan.FromSeconds(3);

        private readonly LoginPage _loginPage;
        private readonly ManagerPage _managerPage;
        private readonly OpenAccountPage _openAccountPage;

        private string _alertText;

        public OpenAccountSteps(LoginPage loginPage, ManagerPage managerPage, OpenAccountPage openAccountPage)
        {
            _loginPage = loginPage;
            _managerPage = managerPage;
            _openAccountPage = openAccountPage;
        }

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

        [Then(@"the account creation message is shown")]
        public void ThenTheAccountCreationMessageIsShown()
        {
            _alertText = _openAccountPage.ReadAndAcceptAlert(AlertTimeout);

            Assert.IsNotNull(_alertText, "Повідомлення про створення рахунку не зʼявилося.");
            StringAssert.Contains(_alertText, "Account created successfully");
        }

        [Then(@"the created account number is not empty")]
        public void ThenTheCreatedAccountNumberIsNotEmpty()
        {
            string accountNumber = OpenAccountPage.ExtractAccountNumber(_alertText);

            Assert.IsFalse(string.IsNullOrWhiteSpace(accountNumber), "У повідомленні немає номера рахунку: " + _alertText);
            Assert.IsTrue(accountNumber.All(char.IsDigit), "Номер рахунку не з цифр: " + accountNumber);
        }

        [Then(@"no account creation message is shown")]
        public void ThenNoAccountCreationMessageIsShown()
        {
            string text = _openAccountPage.ReadAndAcceptAlert(NoAlertTimeout);
            Assert.IsNull(text, "Рахунок не мав створитися, але зʼявилося повідомлення: " + text);
        }

        [Then(@"I stay on the Open Account page")]
        public void ThenIStayOnTheOpenAccountPage()
        {
            StringAssert.Contains(_openAccountPage.CurrentUrl, "#/manager/openAccount");
        }

        [Then(@"the customer list contains:")]
        public void ThenTheCustomerListContains(Table table)
        {
            var actual = _openAccountPage.AvailableCustomers;

            foreach (var row in table.Rows)
                Assert.IsTrue(actual.Contains(row["customer"]), "У списку клієнтів немає: " + row["customer"]);
        }
    }
}
