using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalizerClassLibrary.Tests.Data
{
    /// <summary>
    /// Підняття екземпляра LocalDB перед зверненням до бази.
    /// </summary>
    /// <remarks>
    /// LocalDB зупиняє екземпляр після кількох хвилин простою. Автозапуск
    /// під час підключення в цьому середовищі спрацьовує ненадійно і падає з
    /// «error: 50 — Local Database Runtime error», тому запускаємо явно.
    /// </remarks>
    internal static class LocalDbHelper
    {
        private static readonly object _sync = new object();
        private static string _resolvedExecutable;
        private static bool _executableResolved;

        /// <summary>Ім'я екземпляра з рядка підключення або <c>null</c>.</summary>
        public static string GetInstanceName(string connectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);

                Match match = Regex.Match(
                    builder.DataSource ?? string.Empty,
                    @"^\(localdb\)\\(?<name>.+)$",
                    RegexOptions.IgnoreCase);

                return match.Success ? match.Groups["name"].Value : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Намагається запустити екземпляр. Нічого не кидає: якщо не вийшло,
        /// подробиці все одно потраплять у діагностику через <see cref="Describe"/>.
        /// </summary>
        public static void TryStart(string connectionString)
        {
            string instance = GetInstanceName(connectionString);
            if (instance == null)
                return;

            string executable = ResolveExecutable();
            if (executable == null)
                return;

            RunTool(executable, "start \"" + instance + "\"");
        }

        /// <summary>
        /// Стан середовища для повідомлення про помилку: без цього неможливо
        /// зрозуміти, чому підключення не вдалося саме в цьому процесі.
        /// </summary>
        public static string Describe(string connectionString)
        {
            var report = new StringBuilder();
            string instance = GetInstanceName(connectionString);
            string executable = ResolveExecutable();

            report.AppendLine("--- Діагностика середовища ---");
            report.AppendLine("Рядок підключення: " + connectionString);
            report.AppendLine("Процес: " + Process.GetCurrentProcess().ProcessName +
                              (Environment.Is64BitProcess ? " (64-біт)" : " (32-БІТ)"));
            report.AppendLine("Екземпляр LocalDB: " + (instance ?? "<не LocalDB>"));
            report.AppendLine("SqlLocalDB.exe: " + (executable ?? "НЕ ЗНАЙДЕНО"));

            if (instance != null && executable != null)
            {
                report.AppendLine("SqlLocalDB info " + instance + ":");
                foreach (string line in RunTool(executable, "info \"" + instance + "\"")
                                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Where(l => l.Trim().Length > 0))
                {
                    report.AppendLine("    " + line.Trim());
                }
            }

            return report.ToString();
        }

        private static string RunTool(string executable, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo(executable, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                        return string.Empty;

                    string output = process.StandardOutput.ReadToEnd() +
                                    process.StandardError.ReadToEnd();
                    process.WaitForExit(60000);
                    return output;
                }
            }
            catch (Exception ex)
            {
                return "<не вдалося виконати: " + ex.Message + ">";
            }
        }

        /// <summary>
        /// Шукає SqlLocalDB.exe. У 32-бітному процесі
        /// <c>SpecialFolder.ProgramFiles</c> вказує на «Program Files (x86)»,
        /// де інструмента немає, тому перевіряємо обидві теки явно.
        /// </summary>
        private static string ResolveExecutable()
        {
            lock (_sync)
            {
                if (_executableResolved)
                    return _resolvedExecutable;

                _executableResolved = true;
                _resolvedExecutable = SearchExecutable();
                return _resolvedExecutable;
            }
        }

        private static string SearchExecutable()
        {
            var roots = new List<string>();

            foreach (string variable in new[] { "ProgramW6432", "ProgramFiles", "ProgramFiles(x86)" })
            {
                string value = Environment.GetEnvironmentVariable(variable);
                if (!string.IsNullOrEmpty(value))
                    roots.Add(Path.Combine(value, "Microsoft SQL Server"));
            }

            roots.Add(@"C:\Program Files\Microsoft SQL Server");

            foreach (string root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(root))
                    continue;

                try
                {
                    // найсвіжіша версія: 150 -> SQL 2019, 160 -> SQL 2022 тощо
                    string found = Directory
                        .EnumerateFiles(root, "SqlLocalDB.exe", SearchOption.AllDirectories)
                        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                        .LastOrDefault();

                    if (found != null)
                        return found;
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }

            return null;
        }
    }
}
