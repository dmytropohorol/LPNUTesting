using System;
using System.Text;

namespace AnalizerClassLibrary.Tests.Data
{
    /// <summary>
    /// Один тестовий випадок для методу <c>AnalaizerClass.Format()</c>,
    /// прочитаний з таблиці <c>dbo.FormatTestCase</c> бази даних CalcTestDb.
    /// </summary>
    public sealed class FormatTestCase
    {
        public int TestCaseId { get; set; }

        /// <summary>Код тест-кейса, напр. TC-05-03.</summary>
        public string CaseCode { get; set; }

        /// <summary>Клас еквівалентності вхідних даних.</summary>
        public string CategoryName { get; set; }

        /// <summary>Гілка методу Format(), яку покриває цей тест.</summary>
        public string CoveredBranch { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Вхідний вираз. Будується як <see cref="InputPattern"/>, повторений
        /// <see cref="InputRepeat"/> разів — це дозволяє зберігати в БД
        /// граничні випадки довжини (65536 / 65537 символів) компактно.
        /// </summary>
        public string InputPattern { get; set; }

        public int InputRepeat { get; set; }

        /// <summary>Ім'я очікуваної константи помилки або <c>null</c>.</summary>
        public string ExpectedErrorName { get; set; }

        /// <summary>Позиція символу для помилок з маркером &lt;i&gt;.</summary>
        public int? ExpectedPosition { get; set; }

        /// <summary>Очікуваний відформатований вираз або <c>null</c>.</summary>
        public string ExpectedOutputPattern { get; set; }

        public int ExpectedOutputRepeat { get; set; }

        /// <summary>Чи очікується від методу повідомлення про помилку.</summary>
        public bool ExpectsError
        {
            get { return ExpectedErrorName != null; }
        }

        /// <summary>Розгорнутий вхідний вираз, що подається методу Format().</summary>
        public string Input
        {
            get { return Repeat(InputPattern, InputRepeat); }
        }

        /// <summary>
        /// Повне очікуване значення, яке має повернути Format():
        /// відформатований вираз або «&amp;» + текст повідомлення про помилку.
        /// </summary>
        public string ExpectedResult
        {
            get
            {
                return ExpectsError
                    ? ErrorCatalog.BuildExpectedError(ExpectedErrorName, ExpectedPosition)
                    : Repeat(ExpectedOutputPattern, ExpectedOutputRepeat);
            }
        }

        private static string Repeat(string pattern, int count)
        {
            if (pattern == null) return null;
            if (count == 1) return pattern;
            if (count == 0 || pattern.Length == 0) return string.Empty;

            var sb = new StringBuilder(pattern.Length * count);
            for (int i = 0; i < count; i++) sb.Append(pattern);
            return sb.ToString();
        }

        /// <summary>Коротка назва тесту у звіті про виконання.</summary>
        public override string ToString()
        {
            return string.Format("{0} [{1}] {2}", CaseCode, CategoryName, Description);
        }
    }
}
