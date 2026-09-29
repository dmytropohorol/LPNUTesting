using Microsoft.VisualStudio.TestTools.UnitTesting;

// Метод Format() працює з ПУБЛІЧНИМ СТАТИЧНИМ полем AnalaizerClass.expression,
// тому тести не можна виконувати паралельно — вони спільно використовують
// один і той самий стан класу.
[assembly: DoNotParallelize]
