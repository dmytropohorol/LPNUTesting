using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalcLibTests
{
    [TestClass]
    public class StringAssertMethods
    {
        [TestMethod]
        public void StringContainsTest()
        {

            StringAssert.Contains("Assert samples", "sam");
        }

        [TestMethod]
        public void StringMatchesTest()
        {

            StringAssert.Matches("123", new Regex(@"\d{3}"));
        }


        [TestMethod]
        public void StringStartsWithTest()
        {

            StringAssert.StartsWith("Hello world", "Hello");
        }

        [TestMethod]
        public void StringEndsWithTest()
        {

            StringAssert.EndsWith("Hello world", "world");
        }
    }
}
