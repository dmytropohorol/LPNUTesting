using System;
using System.Collections.Generic;
using System.Reflection;
using AnalaizerClassLibrary;
using AnalizerClassLibrary.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalizerClassLibrary.Tests
{
    // Тести методу AnalaizerClass.Format(). Дані з таблиці dbo.FormatTestCase.
    [TestClass]
    public class FormatTests
    {
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
            AnalaizerClass.ShowMessage = false;
            AnalaizerClass.expression = string.Empty;
        }

        [TestMethod]
        [DynamicData(nameof(FormatCases), DynamicDataDisplayName = nameof(FormatCaseName))]
        public void Format_ReturnsExpectedResult(FormatTestCase testCase)
        {
            AnalaizerClass.expression = testCase.Input;

            string actual = AnalaizerClass.Format();

            Assert.AreEqual(testCase.ExpectedResult, actual, BuildMessage(testCase, actual));
        }

        // Специфікація: повідомлення про помилку починається з «&», вираз ніколи.
        [TestMethod]
        [DynamicData(nameof(FormatCases), DynamicDataDisplayName = nameof(FormatCaseName))]
        public void Format_ErrorResultsStartWithAmpersand(FormatTestCase testCase)
        {
            AnalaizerClass.expression = testCase.Input;

            string actual = AnalaizerClass.Format();

            Assert.AreEqual(testCase.ExpectsError, actual.StartsWith("&", StringComparison.Ordinal),
                BuildMessage(testCase, actual));
        }

        // Format() записує вираз без пробілів у поле expression, звідти його беруть
        // CreateStack() і RunEstimate().
        [TestMethod]
        [DynamicData(nameof(FormatCases), DynamicDataDisplayName = nameof(FormatCaseName))]
        public void Format_RemovesAllSpacesFromExpressionField(FormatTestCase testCase)
        {
            AnalaizerClass.expression = testCase.Input;
            AnalaizerClass.Format();

            Assert.AreEqual(testCase.Input.Replace(" ", string.Empty), AnalaizerClass.expression, testCase.ToString());
        }

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
            return string.Format("{0}{1}Гілка: {2}{1}Вхід: {3}{1}Отримано: {4}",
                testCase, Environment.NewLine, testCase.CoveredBranch,
                Shorten(testCase.Input), Shorten(actual));
        }

        // Граничні випадки мають 65536 символів, у повідомленні вони не потрібні цілком.
        private static string Shorten(string value)
        {
            const int maxLength = 80;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "... (" + value.Length + ")";
        }
    }
}
