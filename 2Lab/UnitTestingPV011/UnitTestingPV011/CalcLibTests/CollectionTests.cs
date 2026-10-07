using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalcLibTests
{
    [TestClass]
    public class CollectionAssertMethods
    {

        static List<string> employees;

        [ClassInitialize]
        public static void InitializeCurrentTest(TestContext TestContext)
        {
            employees = new List<string>();

            employees.Add("Ivan");
            employees.Add("Sergey");
            employees.Add("Anton");
            employees.Add("Roman");
        }


        [TestMethod]
        public void AllItemsAreNotNull()
        {

            CollectionAssert.AllItemsAreNotNull(employees, "Not null failed");
        }

        [TestMethod]
        public void AllItemsAreUnique()
        {

            CollectionAssert.AllItemsAreUnique(employees, "Uniqueness failed");
        }

        [TestMethod]
        public void AreEqual()
        {
            List<string> employeesTest = new List<string>();

            employeesTest.Add("Sergey");
            employeesTest.Add("Ivan");
            employeesTest.Add("Anton");
            employeesTest.Add("Roman");


            CollectionAssert.AreEqual(employees, employeesTest);

        }

        [TestMethod]
        public void AreEquivalent()
        {

            List<string> employeesTest = new List<string>();

            employeesTest.Add("Sergey");
            employeesTest.Add("Ivan");
            employeesTest.Add("Anton");
            employeesTest.Add("Roman");


            CollectionAssert.AreEquivalent(employees, employeesTest);
        }

        [TestMethod]
        public void employees_Subset()
        {
            List<string> employees_Subset = new List<string>();

            employees_Subset.Add(employees[2]);
            employees_Subset.Add("Alexander");


            CollectionAssert.IsSubsetOf(employees_Subset, employees, "failed!");
        }

    }
}
