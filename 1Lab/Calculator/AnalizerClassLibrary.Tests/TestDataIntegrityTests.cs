using System;
using System.Collections.Generic;
using System.Linq;
using AnalizerClassLibrary.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalizerClassLibrary.Tests
{
    // Узгодженість тестових даних у базі з кодом, що тестується.
    [TestClass]
    public class TestDataIntegrityTests
    {
        // ERROR_06, ERROR_08 і ERROR_09 виникають у CalcClass і RunEstimate, не у Format().
        private static readonly string[] ErrorsProducedByFormat =
        {
            "ERROR_01", "ERROR_02", "ERROR_03", "ERROR_04", "ERROR_05", "ERROR_07"
        };

        [TestMethod]
        public void TestCaseSet_IsLoadedFromDatabase()
        {
            Assert.IsTrue(TestCaseRepository.GetFormatTestCases().Count >= 100);
        }

        [TestMethod]
        public void ErrorCodeCatalog_MatchesErrorLibraryConstants()
        {
            IDictionary<string, string> fromDatabase = TestCaseRepository.GetErrorCodeCatalog();

            CollectionAssert.AreEquivalent(ErrorCatalog.ErrorNames.ToArray(), fromDatabase.Keys.ToArray());

            foreach (KeyValuePair<string, string> row in fromDatabase)
                Assert.AreEqual(ErrorCatalog.GetMessageTemplate(row.Key), row.Value, row.Key);
        }

        [TestMethod]
        public void EveryCategory_HasAtLeastOneTestCase()
        {
            var used = new HashSet<string>(TestCaseRepository.GetFormatTestCases().Select(c => c.CategoryName));
            string[] unused = TestCaseRepository.GetCategoryNames().Where(name => !used.Contains(name)).ToArray();

            Assert.AreEqual(0, unused.Length, "Категорії без тестів: " + string.Join(", ", unused));
        }

        [TestMethod]
        public void TestCases_HaveUniqueCaseCodes()
        {
            string[] duplicates = TestCaseRepository.GetFormatTestCases()
                .GroupBy(c => c.CaseCode)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            Assert.AreEqual(0, duplicates.Length, "Неунікальні коди: " + string.Join(", ", duplicates));
        }

        [TestMethod]
        public void TestCases_CoverEveryErrorCodeFormatCanReturn()
        {
            var covered = new HashSet<string>(TestCaseRepository.GetFormatTestCases()
                .Where(c => c.ExpectsError)
                .Select(c => c.ExpectedErrorName));

            string[] missing = ErrorsProducedByFormat.Where(name => !covered.Contains(name)).ToArray();

            Assert.AreEqual(0, missing.Length, "Немає тестів для: " + string.Join(", ", missing));
        }

        [TestMethod]
        public void TestCases_CoverBothSuccessfulAndFailingOutcomes()
        {
            IList<FormatTestCase> cases = TestCaseRepository.GetFormatTestCases();

            Assert.IsTrue(cases.Any(c => c.ExpectsError), "Немає жодного негативного випадку.");
            Assert.IsTrue(cases.Any(c => !c.ExpectsError), "Немає жодного позитивного випадку.");
        }

        [TestMethod]
        public void TestCases_WithPosition_UseErrorCodesThatContainPositionMarker()
        {
            string[] invalid = TestCaseRepository.GetFormatTestCases()
                .Where(c => c.ExpectedPosition.HasValue)
                .Where(c => !ErrorCatalog.GetMessageTemplate(c.ExpectedErrorName).Contains("<i>"))
                .Select(c => c.CaseCode)
                .ToArray();

            Assert.AreEqual(0, invalid.Length, "Позиція для помилки без маркера <i>: " + string.Join(", ", invalid));
        }
    }
}
