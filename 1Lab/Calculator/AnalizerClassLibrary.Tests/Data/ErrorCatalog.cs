using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ErrorLibrary;

namespace AnalizerClassLibrary.Tests.Data
{
    /// <summary>
    /// Доступ до текстів повідомлень про помилки за іменем константи
    /// (ERROR_01 ... ERROR_09).
    ///
    /// У базі даних зберігається лише ІМ'Я константи, а не її текст. Завдяки
    /// цьому текст повідомлення має одне-єдине джерело правди —
    /// <see cref="ErrorsExpression"/>, і тест не «ламається» від зміни
    /// формулювання, але перевіряє правильність вибору коду помилки.
    /// </summary>
    public static class ErrorCatalog
    {
        private static readonly IDictionary<string, string> _messages = LoadMessages();

        private static IDictionary<string, string> LoadMessages()
        {
            return typeof(ErrorsExpression)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Where(f => f.Name.StartsWith("ERROR_", StringComparison.Ordinal))
                .ToDictionary(f => f.Name, f => (string)f.GetRawConstantValue(), StringComparer.Ordinal);
        }

        /// <summary>Імена всіх констант помилок, оголошених у ErrorLibrary.</summary>
        public static IEnumerable<string> ErrorNames
        {
            get { return _messages.Keys; }
        }

        /// <summary>Шаблон повідомлення за іменем константи.</summary>
        public static string GetMessageTemplate(string errorName)
        {
            string message;
            if (!_messages.TryGetValue(errorName, out message))
            {
                throw new KeyNotFoundException(
                    string.Format(
                        "У класі ErrorsExpression немає константи '{0}'. " +
                        "Перевірте значення ErrorCode.ErrorName у базі даних CalcTestDb.",
                        errorName));
            }

            return message;
        }

        /// <summary>
        /// Формує рядок, який метод Format() повертає у разі помилки:
        /// спец. символ '&amp;' + текст повідомлення, у якому маркер
        /// &lt;i&gt; замінено позицією символу (якщо позиція задана).
        /// </summary>
        public static string BuildExpectedError(string errorName, int? position)
        {
            string template = GetMessageTemplate(errorName);

            return "&" + (position.HasValue
                ? ErrorsExpression.GetFullStringError(template, position.Value)
                : template);
        }
    }
}
