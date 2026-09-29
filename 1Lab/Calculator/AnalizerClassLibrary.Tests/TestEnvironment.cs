using System;
using AnalizerClassLibrary.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalizerClassLibrary.Tests
{
    /// <summary>
    /// Підготовка середовища перед запуском тестів.
    /// </summary>
    /// <remarks>
    /// LocalDB зупиняє екземпляр після простою, а його автозапуск під час
    /// підключення спрацьовує не завжди. Тому перед першим тестом піднімаємо
    /// екземпляр явно — інакше запуск з Test Explorer падає з помилкою
    /// «error: 50 — Local Database Runtime error», хоча база на місці.
    ///
    /// Метод навмисно «м'який»: будь-яка проблема тут не має валити весь
    /// прогін — якщо підняти не вдалося, тест впаде далі з детальним
    /// повідомленням від <see cref="TestCaseRepository"/>.
    /// </remarks>
    [TestClass]
    public class TestEnvironment
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            try
            {
                EnsureLocalDbStarted(TestCaseRepository.ConnectionString);
            }
            catch
            {
                // best-effort: детальну діагностику видасть перший же тест
            }
        }

        private static void EnsureLocalDbStarted(string connectionString)
        {
            LocalDbHelper.TryStart(connectionString);
        }
    }
}
