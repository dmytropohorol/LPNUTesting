using System;
using CalcLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalcLibTests
{
    [TestClass]
    public class CalcTests
    {
        public TestContext TestContext { get; set; }

        [DataSource("System.Data.SqlClient", @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\VS_Projects\UnitTestingPV011\CalcLibTests\bin\Debug\TestCalcPV011DB.mdf;Integrated Security=True;Connect Timeout=30","TestData",DataAccessMethod.Sequential)]
        [TestMethod]
        public void Calc_Div_FromDB()
        {
            double a = (double)TestContext.DataRow["a"];
            double b = (double)TestContext.DataRow["b"];
            string r = (string)TestContext.DataRow["r"];
            double actual;
            double delta = 0.01;
            try
            {
                actual = Calc.Div(a, b);
                Assert.AreEqual(Convert.ToDouble(r), actual, delta);
            }
            catch(DivideByZeroException ex)
            {
                Assert.AreEqual(r, "Error");
            }
        }


        [TestMethod]
        public void Sum_10plus20_30returned()
        {
            //AAA
            //Arrange
            double a = 10;
            double b = 20;
            double expected = 30;
            //Actual
            double actual = Calc.Sum(a, b);
            //Assert
            Assert.AreEqual(expected, actual);

        }
        [TestMethod]
        public void Div_10div2_5returned()
        {
            //AAA
            //Arrange
            double a = 10;
            double b = 2;
            double expected = 5;
            //Actual
            double actual = Calc.Div(a, b);
            //Assert
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void Div_10div3_3dot33returned()
        {
            //AAA
            //Arrange
            double a = 10;
            double b = 3;
            double expected = 3.33;
            double delta = 0.01;
            //Actual
            double actual = Calc.Div(a, b);
            //Assert
            Assert.AreEqual(expected, actual,delta);
        }
        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Div_10div0_Exception()
        {
            //Arrange
            double a = 10;
            double b = 0;
            //Actual
            double actual = Calc.Div(a, b);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void Test1()
        {
            //Arrange
            double a = double.MaxValue;
            double b = double.MaxValue;
            //Actual
            double actual = Calc.Sum(a, b);
        }

    }
}
