using System;
using CalcLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalcLibTests
{
    [TestClass]
    public class UserManagerTest
    {
        public TestContext TestContext { get; set; }
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"D:\VS_Projects\UnitTestingPV011\CalcLibTests\testData.xml", "User",DataAccessMethod.Sequential)]
        [TestMethod]
        public void UserManager_Add_FromXML()
        {
            string userid = Convert.ToString(TestContext.DataRow["userid"]);
            string phone = Convert.ToString(TestContext.DataRow["phone"]);
            string email = Convert.ToString(TestContext.DataRow["email"]);
            UserManager manager = new UserManager();
          bool result=  manager.Add(userid, phone, email);
            Assert.IsTrue(result);
        }
    }
}
