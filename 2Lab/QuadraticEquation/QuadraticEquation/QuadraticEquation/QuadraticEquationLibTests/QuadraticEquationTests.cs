using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuadraticEquationLib;
using System;

namespace QuadraticEquationLibTests
{
    [TestClass]
    public class QuadraticEquationTests
    {
        double a, b, c;
        (double, double) actual;
        string rootsString;

        public TestContext TestContext { get; set; }
        [DataSource(
            "System.Data.SqlClient",
            @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|TestQuadraticEquation.mdf;Integrated Security=True;Connect Timeout=30",
            "DataAndResults",
            DataAccessMethod.Sequential
            )]


        [TestMethod]
        public void Solving_FromBD()
        {
            a = (double)TestContext.DataRow["a"];
            b = (double)TestContext.DataRow["b"];
            c = (double)TestContext.DataRow["c"];
            rootsString = (string)TestContext.DataRow["roots"];

            try
            {
                actual = QuadraticEquation.Solving(a, b, c);
                string[] rootsArr = rootsString.Split('~');
                (double, double) roots = (Convert.ToDouble(rootsArr[0]), Convert.ToDouble(rootsArr[1]));

                Assert.AreEqual(roots, actual);
            }
            catch
            {
                Assert.AreEqual(rootsString, "Error");
            }

        }
    }
}
