using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace AnalizerClassLibrary.Tests.Data
{
    /// <summary>
    /// Читає набір тестових даних з бази даних MS SQL Server CalcTestDb.
    /// </summary>
    /// <remarks>
    /// Рядок підключення береться зі змінної середовища
    /// <c>CALC_TESTDB_CONNECTION</c>, а якщо її не задано — з App.config
    /// (секція connectionStrings, ім'я «CalcTestDb»).
    /// </remarks>
    public static class TestCaseRepository
    {
        private const string ConnectionName = "CalcTestDb";
        private const string EnvironmentVariable = "CALC_TESTDB_CONNECTION";

        /// <summary>Скільки разів повторити спробу звернення до БД.</summary>
        private const int ConnectionAttempts = 3;

        /// <summary>Пауза між спробами, мс.</summary>
        private const int RetryDelayMilliseconds = 3000;

        private const string DeployHint =
            "Розгорніть базу даних скриптом Database\\Deploy-TestDatabase.ps1 " +
            "або перевірте рядок підключення в App.config / змінній середовища " +
            EnvironmentVariable + ".";

        private static readonly object _sync = new object();
        private static IList<FormatTestCase> _cache;
        private static Exception _cachedFailure;

        public static string ConnectionString
        {
            get
            {
                string fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(fromEnvironment))
                    return fromEnvironment;

                ConnectionStringSettings settings =
                    ConfigurationManager.ConnectionStrings[ConnectionName];

                if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "Не знайдено рядок підключення '" + ConnectionName + "'. " + DeployHint);
                }

                return settings.ConnectionString;
            }
        }

        /// <summary>
        /// Усі активні тест-кейси методу Format() у порядку зростання TestCaseId.
        /// Результат кешується: БД опитується один раз на запуск тестів.
        /// </summary>
        public static IList<FormatTestCase> GetFormatTestCases()
        {
            lock (_sync)
            {
                // Атрибут [DynamicData] обчислюється окремо для КОЖНОГО тестового
                // методу. Якщо база недоступна, без кешування помилки кожен метод
                // наново витрачав би весь бюджет таймаутів і повторів, і Test Explorer
                // «зависав» би на багато хвилин. Тому перший невдалий результат теж
                // запам'ятовуємо і далі повертаємо миттєво.
                if (_cachedFailure != null)
                    throw new InvalidOperationException(_cachedFailure.Message, _cachedFailure);

                if (_cache == null)
                {
                    try
                    {
                        _cache = LoadFormatTestCases();
                    }
                    catch (Exception ex)
                    {
                        _cachedFailure = ex;
                        throw;
                    }
                }

                return _cache;
            }
        }

        /// <summary>
        /// Джерело даних для атрибута <c>[DynamicData]</c>.
        /// </summary>
        public static IEnumerable<object[]> GetFormatTestCaseRows()
        {
            foreach (FormatTestCase testCase in GetFormatTestCases())
                yield return new object[] { testCase };
        }

        /// <summary>
        /// Довідник кодів помилок з БД: ім'я константи -> шаблон повідомлення.
        /// Використовується для перевірки узгодженості БД і ErrorLibrary.
        /// </summary>
        public static IDictionary<string, string> GetErrorCodeCatalog()
        {
            return Query(
                    "SELECT ErrorName, MessageTemplate FROM dbo.ErrorCode ORDER BY ErrorCodeId;",
                    reader => new KeyValuePair<string, string>(reader.GetString(0), reader.GetString(1)))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }

        /// <summary>Назви всіх категорій тестів, оголошених у БД.</summary>
        public static IList<string> GetCategoryNames()
        {
            return Query(
                "SELECT CategoryName FROM dbo.TestCategory ORDER BY TestCategoryId;",
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

            IList<FormatTestCase> result = Query(sql, reader => new FormatTestCase
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

            if (result.Count == 0)
            {
                throw new InvalidOperationException(
                    "Таблиця dbo.FormatTestCase не містить жодного активного тест-кейса. " + DeployHint);
            }

            return result;
        }

        /// <summary>
        /// Виконує запит і проєціює кожен рядок у об'єкт <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// LocalDB зупиняє екземпляр після простою і запускає його автоматично під
        /// час наступного підключення. Поки екземпляр піднімається, підключення може
        /// впасти з помилкою 4060 («Cannot open database … Login failed»), тому першу
        /// невдачу не вважаємо остаточною. Список будується заново на кожній спробі,
        /// тож повтор не може додати рядки двічі.
        /// </remarks>
        private static IList<T> Query<T>(string sql, Func<IDataRecord, T> projector)
        {
            SqlException lastError = null;

            for (int attempt = 1; attempt <= ConnectionAttempts; attempt++)
            {
                var rows = new List<T>();

                try
                {
                    using (var connection = new SqlConnection(ConnectionString))
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandTimeout = 120;
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                rows.Add(projector(reader));
                        }
                    }

                    return rows;
                }
                catch (SqlException ex)
                {
                    lastError = ex;

                    if (attempt < ConnectionAttempts)
                    {
                        // Найчастіша причина — екземпляр LocalDB зупинився після
                        // простою, а автозапуск під час підключення не спрацював.
                        LocalDbHelper.TryStart(ConnectionString);
                        Thread.Sleep(RetryDelayMilliseconds);
                    }
                }
            }

            throw new InvalidOperationException(
                string.Format(
                    "Не вдалося прочитати тестові дані з бази даних CalcTestDb ({0} спроби). {1}{2}" +
                    "Помилка SQL Server: {3}{2}{4}",
                    ConnectionAttempts, DeployHint, Environment.NewLine, lastError.Message,
                    LocalDbHelper.Describe(ConnectionString)),
                lastError);
        }
    }
}
