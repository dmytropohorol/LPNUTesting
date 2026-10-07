using System;
using CalcLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalcLibTests
{
    [TestClass]
    public class MyClassTests
    {
        [TestMethod]
        public void IsSqrtTest()
        {
            // arrange
            const double input = 4;
            const double expected = 2;
            // act
            double actual = MyClass.GetSqrt(input);
            // assert
            Assert.AreEqual(expected, actual, "Sqrt of {0} should have been {1}!", input, expected);
        }

        [TestMethod]
        public void StringAreEqualTest()
        {
            // arrange
            const string input = "HELLO";
            const string expected = "hello";

            // act and assert

            Assert.AreEqual(expected, input, true);

        }
        [TestMethod]
        public void StringSameTest()
        {
             //string a = "Hello";
             //string b = "Hello";
            int a = 5;
            int b = 5;

            Assert.AreSame(a, b);
        }
    }
}
