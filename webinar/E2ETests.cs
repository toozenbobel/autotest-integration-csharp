using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace webinar
{
    [TestClass]
    public class E2ETests
    {
        [CustomTestMethod]
        [TestCaseId(9)]
        public void Test1()
        {
          	Thread.Sleep(5000);
            Assert.IsTrue(false);
        }

        [CustomTestMethod]
        [TestCaseId(8)]
        public void Test2()
        {
          	Thread.Sleep(5000);
            Assert.IsTrue(true);
        }
      
      	[CustomTestMethod]
        [TestCaseId(5)]
        public void Test3()
        {
          	Thread.Sleep(5000);
            Assert.IsTrue(true);
        }
    }
}
