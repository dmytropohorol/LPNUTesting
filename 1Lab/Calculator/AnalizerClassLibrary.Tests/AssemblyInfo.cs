using Microsoft.VisualStudio.TestTools.UnitTesting;

// Format() працює зі спільним статичним полем AnalaizerClass.expression,
// тому паралельний запуск тестів неможливий.
[assembly: DoNotParallelize]
