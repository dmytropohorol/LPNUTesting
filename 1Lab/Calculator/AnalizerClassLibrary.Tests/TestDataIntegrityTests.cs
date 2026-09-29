using System;
using System.Collections.Generic;
using System.Linq;
using AnalizerClassLibrary.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalizerClassLibrary.Tests
{
    /// <summary>
    /// Перевірки узгодженості набору тестових даних у базі CalcTestDb
    /// з кодом бібліотек, що тестуються. Якщо специфікацію або код помилок
    /// змінять, ці тести впадуть першими й покажуть, що БД треба оновити.
    /// </summary>
    [TestClass]
    public class TestDataIntegrityTests
    {
        /// <summary>
        /// Коди помилок, які взагалі здатен повернути метод Format().
        /// ERROR_06, ERROR_08 та ERROR_09 виникають на інших етапах обчислення
        /// (CalcClass та RunEstimate) і до зони відповідальності Format() не належать.
        /// </summary>
        private static readonly string[] ErrorsProducedByFormat =
        {
            "ERROR_01", "ERROR_02", "ERROR_03", "ERROR_04", "ERROR_05", "ERROR_07"
        };

        [TestMethod]
        public void TestCaseSet_IsLoadedFromDatabase()
        {
            IList<FormatTestCase> cases = TestCaseRepository.GetFormatTestCases();

            Assert.IsTrue(
                cases.Count >= 100,
                "Очікується щонайменше 100 тест-кейсів у dbo.FormatTestCase, прочитано {0}.",
                cases.Count);
        }

        [TestMethod]
        public void ErrorCodeCatalog_MatchesErrorLibraryConstants()
        {
            IDictionary<string, string> fromDatabase = TestCaseRepository.GetErrorCodeCatalog();

            CollectionAssert.AreEquivalent(
                ErrorCatalog.ErrorNames.ToArray(),
                fromDatabase.Keys.ToArray(),
                "Перелік кодів помилок у таблиці dbo.ErrorCode не збігається " +
                "з константами класу ErrorLibrary.ErrorsExpression.");

            foreach (KeyValuePair<string, string> row in fromDatabase)
            {
                Assert.AreEqual(
                    ErrorCatalog.GetMessageTemplate(row.Key),
                    row.Value,
                    "Текст повідомлення {0} у базі даних відрізняється від константи " +
                    "ErrorsExpression.{0}.",
                    row.Key);
            }
        }

        [TestMethod]
        public void EveryCategory_HasAtLeastOneTestCase()
        {
            IList<string> declared = TestCaseRepository.GetCategoryNames();

            HashSet<string> used = new HashSet<string>(
                TestCaseRepository.GetFormatTestCases().Select(c => c.CategoryName),
                StringComparer.Ordinal);

            string[] unused = declared.Where(name => !used.Contains(name)).ToArray();

            Assert.AreEqual(
                0,
                unused.Length,
                "Категорії без жодного активного тест-кейса: {0}.",
                string.Join(", ", unused));
        }

        [TestMethod]
        public void TestCases_HaveUniqueCaseCodes()
        {
            string[] duplicates = TestCaseRepository.GetFormatTestCases()
                .GroupBy(c => c.CaseCode, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            Assert.AreEqual(
                0,
                duplicates.Length,
                "Знайдено неунікальні коди тест-кейсів: {0}.",
                string.Join(", ", duplicates));
        }

        [TestMethod]
        public void TestCases_CoverEveryErrorCodeFormatCanReturn()
        {
            HashSet<string> covered = new HashSet<string>(
                TestCaseRepository.GetFormatTestCases()
                    .Where(c => c.ExpectsError)
                    .Select(c => c.ExpectedErrorName),
                StringComparer.Ordinal);

            string[] missing = ErrorsProducedByFormat
                .Where(name => !covered.Contains(name))
                .ToArray();

            Assert.AreEqual(
                0,
                missing.Length,
                "Немає жодного тест-кейса для кодів помилок: {0}.",
                string.Join(", ", missing));
        }

        [TestMethod]
        public void TestCases_CoverBothSuccessfulAndFailingOutcomes()
        {
            IList<FormatTestCase> cases = TestCaseRepository.GetFormatTestCases();

            int failing = cases.Count(c => c.ExpectsError);
            int successful = cases.Count - failing;

            Assert.IsTrue(failing > 0, "У наборі немає жодного негативного тест-кейса.");
            Assert.IsTrue(successful > 0, "У наборі немає жодного позитивного тест-кейса.");
        }

        [TestMethod]
        public void TestCases_WithPosition_UseErrorCodesThatContainPositionMarker()
        {
            string[] invalid = TestCaseRepository.GetFormatTestCases()
                .Where(c => c.ExpectedPosition.HasValue)
                .Where(c => !ErrorCatalog.GetMessageTemplate(c.ExpectedErrorName).Contains("<i>"))
                .Select(c => c.CaseCode)
                .ToArray();

            Assert.AreEqual(
                0,
                invalid.Length,
                "Позицію символу задано для помилок без маркера <i>: {0}.",
                string.Join(", ", invalid));
        }
    }
}
