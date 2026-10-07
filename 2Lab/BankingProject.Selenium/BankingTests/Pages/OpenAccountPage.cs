using System;
using System.Collections.Generic;
using System.Linq;
using BankingTests.Drivers;
using OpenQA.Selenium;

namespace BankingTests.Pages
{
    /// <summary>
    /// Вкладка Open Account: вибір клієнта, вибір валюти та створення рахунку.
    /// Результат операції застосунок повідомляє через діалог alert браузера.
    /// </summary>
    public class OpenAccountPage : BasePage
    {
        private static readonly By CustomerSelect = By.Id("userSelect");
        private static readonly By CurrencySelect = By.Id("currency");

        // Кнопку Process шукаємо всередині форми: у шапці сторінки є інші
        // кнопки з type='submit', і без форми локатор був би неоднозначним.
        private static readonly By ProcessButton =
            By.CssSelector("form[name='myForm'] button[type='submit']");

        public OpenAccountPage(BrowserDriver browser) : base(browser)
        {
        }

        public bool IsOpen
        {
            get { return IsVisible(ProcessButton, BrowserDriver.DefaultTimeout); }
        }

        /// <summary>Перелік клієнтів у випадному списку, без рядка підказки.</summary>
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

        public string SelectedCustomer
        {
            get { return Dropdown(CustomerSelect).SelectedOption.Text.Trim(); }
        }

        public void Process()
        {
            Click(ProcessButton);
        }

        /// <summary>
        /// Читає текст діалогу alert і закриває його.
        /// </summary>
        /// <returns>Текст повідомлення або <c>null</c>, якщо діалог не з'явився.</returns>
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

        /// <summary>
        /// Дістає номер рахунку з повідомлення виду
        /// «Account created successfully with account Number :1016».
        /// </summary>
        public static string ExtractAccountNumber(string alertText)
        {
            if (string.IsNullOrEmpty(alertText))
                return string.Empty;

            int separator = alertText.LastIndexOf(':');
            return separator < 0
                ? string.Empty
                : alertText.Substring(separator + 1).Trim();
        }
    }
}
