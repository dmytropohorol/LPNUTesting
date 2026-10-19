using System.Text;

namespace AnalizerClassLibrary.Tests.Data
{
    // Один рядок таблиці dbo.FormatTestCase.
    public sealed class FormatTestCase
    {
        public int TestCaseId { get; set; }
        public string CaseCode { get; set; }
        public string CategoryName { get; set; }
        public string CoveredBranch { get; set; }
        public string Description { get; set; }

        // Вхід = InputPattern, повторений InputRepeat разів: так граничні
        // випадки на 65536 і 65537 символів займають у таблиці по одному рядку.
        public string InputPattern { get; set; }
        public int InputRepeat { get; set; }

        public string ExpectedErrorName { get; set; }
        public int? ExpectedPosition { get; set; }
        public string ExpectedOutputPattern { get; set; }
        public int ExpectedOutputRepeat { get; set; }

        public bool ExpectsError
        {
            get { return ExpectedErrorName != null; }
        }

        public string Input
        {
            get { return Repeat(InputPattern, InputRepeat); }
        }

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
            var sb = new StringBuilder(pattern.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(pattern);
            return sb.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}] {2}", CaseCode, CategoryName, Description);
        }
    }
}
