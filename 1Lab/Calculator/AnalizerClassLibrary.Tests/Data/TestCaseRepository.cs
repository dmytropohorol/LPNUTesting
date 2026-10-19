using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AnalizerClassLibrary.Tests.Data
{
    // Читає тестові дані з бази CalcTestDb (MS SQL Server).
    public static class TestCaseRepository
    {
        // [DynamicData] обчислюється для кожного тестового методу окремо, тому
        // результат кешується. Lazy кешує і виняток: якщо бази немає, кожен
        // метод не чекатиме таймаут підключення заново.
        private static readonly Lazy<IList<FormatTestCase>> _cases =
            new Lazy<IList<FormatTestCase>>(LoadFormatTestCases);

        private static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["CalcTestDb"].ConnectionString; }
        }

        public static IList<FormatTestCase> GetFormatTestCases()
        {
            return _cases.Value;
        }

        public static IEnumerable<object[]> GetFormatTestCaseRows()
        {
            return GetFormatTestCases().Select(testCase => new object[] { testCase });
        }

        public static IDictionary<string, string> GetErrorCodeCatalog()
        {
            return Query("SELECT ErrorName, MessageTemplate FROM dbo.ErrorCode ORDER BY ErrorCodeId;",
                    reader => new KeyValuePair<string, string>(reader.GetString(0), reader.GetString(1)))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }

        public static IList<string> GetCategoryNames()
        {
            return Query("SELECT CategoryName FROM dbo.TestCategory ORDER BY TestCategoryId;",
                reader => reader.GetString(0));
        }

        private static IList<FormatTestCase> LoadFormatTestCases()
        {
            const string sql =
                "SELECT TestCaseId, CaseCode, CategoryName, CoveredBranch, Description, " +
                "       InputPattern, InputRepeat, ExpectedErrorName, ExpectedPosition, " +
                "       ExpectedOutputPattern, ExpectedOutputRepeat " +
                "FROM   dbo.vFormatTestCase " +
                "ORDER BY TestCaseId;";

            return Query(sql, reader => new FormatTestCase
            {
                TestCaseId            = reader.GetInt32(0),
                CaseCode              = reader.GetString(1),
                CategoryName          = reader.GetString(2),
                CoveredBranch         = reader.GetString(3),
                Description           = reader.GetString(4),
                InputPattern          = reader.GetString(5),
                InputRepeat           = reader.GetInt32(6),
                ExpectedErrorName     = reader.IsDBNull(7) ? null : reader.GetString(7),
                ExpectedPosition      = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                ExpectedOutputPattern = reader.IsDBNull(9) ? null : reader.GetString(9),
                ExpectedOutputRepeat  = reader.GetInt32(10),
            });
        }

        private static IList<T> Query<T>(string sql, Func<IDataRecord, T> projector)
        {
            var rows = new List<T>();

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        rows.Add(projector(reader));
                }
            }

            return rows;
        }
    }
}
