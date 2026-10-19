using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ErrorLibrary;

namespace AnalizerClassLibrary.Tests.Data
{
    // У базі зберігається лише імʼя константи (ERROR_01...), а текст береться
    // з ErrorsExpression. Так у повідомлень одне джерело правди.
    public static class ErrorCatalog
    {
        private static readonly IDictionary<string, string> _messages = typeof(ErrorsExpression)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string) && f.Name.StartsWith("ERROR_", StringComparison.Ordinal))
            .ToDictionary(f => f.Name, f => (string)f.GetRawConstantValue(), StringComparer.Ordinal);

        public static IEnumerable<string> ErrorNames
        {
            get { return _messages.Keys; }
        }

        public static string GetMessageTemplate(string errorName)
        {
            return _messages[errorName];
        }

        // Те, що Format() повертає при помилці: «&» + текст, де <i> замінено позицією.
        public static string BuildExpectedError(string errorName, int? position)
        {
            string template = GetMessageTemplate(errorName);

            return "&" + (position.HasValue
                ? ErrorsExpression.GetFullStringError(template, position.Value)
                : template);
        }
    }
}
