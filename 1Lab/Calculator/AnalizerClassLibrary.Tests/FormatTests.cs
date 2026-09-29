using System;
using System.Collections.Generic;
using System.Reflection;
using AnalaizerClassLibrary;
using AnalizerClassLibrary.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalizerClassLibrary.Tests
{
    /// <summary>
    /// Лабораторна робота №1, варіант 4:
    /// модульні тести методу <c>AnalaizerClassLibrary.AnalaizerClass.Format()</c>.
    ///
    /// Набір вхідних даних та очікуваних результатів зберігається в базі даних
    /// MS SQL Server <c>CalcTestDb</c> (таблиці dbo.FormatTestCase,
    /// dbo.TestCategory, dbo.ErrorCode) і зчитується через
    /// <see cref="TestCaseRepository"/>.
    /// </summary>
    [TestClass]
    public class FormatTests
    {
        /// <summary>Набір тест-кейсів з БД для атрибута [DynamicData].</summary>
        public static IEnumerable<object[]> FormatCases
        {
            get { return TestCaseRepository.GetFormatTestCaseRows(); }
        }

        public static string FormatCaseName(MethodInfo methodInfo, object[] data)
        {
            return data[0].ToString();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // У консольному/автоматизованому режимі жодних MessageBox бути не повинно
            // (Format() їх не показує, але поле є спільним для всього класу).
            AnalaizerClass.ShowMessage = false;
            AnalaizerClass.expression = string.Empty;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            AnalaizerClass.expression = string.Empty;
        }

        /// <summary>
        /// Основний тест: для кожного рядка таблиці dbo.FormatTestCase метод
        /// Format() має повернути саме те значення, яке задане в БД.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(FormatCases), DynamicDataDisplayName = nameof(FormatCaseName))]
        public void Format_ReturnsExpectedResult(FormatTestCase testCase)
        {
            AnalaizerClass.expression = testCase.Input;

            string actual = AnalaizerClass.Format();

            Assert.AreEqual(
                testCase.ExpectedResult,
                actual,
                BuildMessage(testCase, actual));
        }

        /// <summary>
        /// Контракт методу зі специфікації (п. 2.2.3): повідомлення про помилку
        /// завжди починається зі спец. символу «&amp;», а коректно
        /// відформатований вираз — ніколи.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(FormatCases), DynamicDataDisplayName = nameof(FormatCaseName))]
        public void Format_ErrorResultsStartWithAmpersand(FormatTestCase testCase)
        {
            AnalaizerClass.expression = testCase.Input;

            string actual = AnalaizerClass.Format();

            Assert.AreEqual(
                testCase.ExpectsError,
                actual.StartsWith("&", StringComparison.Ordinal),
                BuildMessage(testCase, actual));
        }

        /// <summary>
        /// Format() не лише повертає відформатований вираз, а й записує його
        /// у статичне поле AnalaizerClass.expression — саме звідти його беруть
        /// CreateStack() і RunEstimate().
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(FormatCases), DynamicDataDisplayName = nameof(FormatCaseName))]
        public void Format_RemovesAllSpacesFromExpressionField(FormatTestCase testCase)
        {
            string expected = testCase.Input.Replace(" ", string.Empty);

            AnalaizerClass.expression = testCase.Input;
            AnalaizerClass.Format();

            Assert.AreEqual(
                expected,
                AnalaizerClass.expression,
                "{0}{1}Поле AnalaizerClass.expression має містити вхідний вираз без пробілів.",
                testCase, Environment.NewLine);
        }

        /// <summary>
        /// Повторний виклик Format() для вже відформатованого виразу має давати
        /// той самий результат (ідемпотентність), бо CreateStack() працює саме
        /// з полем expression, яке Format() перезаписує.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(FormatCases), DynamicDataDisplayName = nameof(FormatCaseName))]
        public void Format_IsIdempotent(FormatTestCase testCase)
        {
            AnalaizerClass.expression = testCase.Input;

            string first = AnalaizerClass.Format();
            string second = AnalaizerClass.Format();

            Assert.AreEqual(first, second, BuildMessage(testCase, second));
        }

        private static string BuildMessage(FormatTestCase testCase, string actual)
        {
            const int maxLength = 80;

            return string.Format(
                "{0}{1}Гілка Format(): {2}{1}Вхід:      {3}{1}Очікувано: {4}{1}Отримано:  {5}",
                testCase,
                Environment.NewLine,
                testCase.CoveredBranch,
                Shorten(testCase.Input, maxLength),
                Shorten(testCase.ExpectedResult, maxLength),
                Shorten(actual, maxLength));
        }

        private static string Shorten(string value, int maxLength)
        {
            if (value == null) return "<null>";
            if (value.Length <= maxLength) return "'" + value + "'";

            return string.Format("'{0}...' (довжина {1})",
                value.Substring(0, maxLength), value.Length);
        }
    }
}
