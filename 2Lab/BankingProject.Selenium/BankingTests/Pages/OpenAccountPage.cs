using System;
using System.Collections.Generic;
using System.Linq;
using BankingTests.Drivers;
using OpenQA.Selenium;

namespace BankingTests.Pages
{
    public class OpenAccountPage : BasePage
    {
        private static readonly By CustomerSelect = By.Id("userSelect");
        private static readonly By CurrencySelect = By.Id("currency");

        // Лише всередині форми: у шапці сторінки є інші кнопки type='submit'.
        private static readonly By ProcessButton = By.CssSelector("form[name='myForm'] button[type='submit']");

        public OpenAccountPage(BrowserDriver browser) : base(browser)
        {
        }

        public bool IsOpen
        {
            get { return IsVisible(ProcessButton, BrowserDriver.DefaultTimeout); }
        }

        public IReadOnlyList<string> AvailableCustomers
        {
            get
            {
                return Dropdown(CustomerSelect).Options
                    .Select(option => option.Text.Trim())
                    .Where(text => !text.StartsWith("---", StringComparison.Ordinal))
                    .ToList();
            }
        }

        public void SelectCustomer(string customerName)
        {
            Dropdown(CustomerSelect).SelectByText(customerName);
        }

        public void SelectCurrency(string currency)
        {
            Dropdown(CurrencySelect).SelectByText(currency);
        }

        public void Process()
        {
            Click(ProcessButton);
        }

        // Результат застосунок повідомляє діалогом alert. null, якщо діалогу немає.
        public string ReadAndAcceptAlert(TimeSpan timeout)
        {
            IAlert alert;

            try
            {
                alert = Browser.CreateWait(timeout).Until(driver =>
                {
                    try
                    {
                        return driver.SwitchTo().Alert();
                    }
                    catch (NoAlertPresentException)
                    {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }

            string text = alert.Text;
            alert.Accept();
            return text;
        }

        // «Account created successfully with account Number :1016» -> «1016»
        public static string ExtractAccountNumber(string alertText)
        {
            if (string.IsNullOrEmpty(alertText))
                return string.Empty;

            int separator = alertText.LastIndexOf(':');
            return separator < 0 ? string.Empty : alertText.Substring(separator + 1).Trim();
        }
    }
}
