using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroServiceInstaller3.Tests
{
    [TestClass]
    public class ServicesHandlerTests
    {
        [TestMethod]
        public void TestInstallService()
        {
            string serviceName = "TestService";
            string displayName = "TestService";
            string fileName = "C:/Users/IEUser/source/repos/Relika/Praktika/Praktika/MicroServiceInstaller3/TestService/bin/Debug/TestService.exe";
            ServiceInstaller.InstallAndStart(serviceName, displayName, fileName);
        }

        [TestMethod]
        public void TestStopService()
        {
            string serviceName = "TestService";
            ServiceInstaller.StopService(serviceName);
            ServiceState serviceStatus = ServiceInstaller.GetServiceStatus(serviceName);
            //string expectedValue = JsonConvert.SerializeObject("1");
            //string actualValue = JsonConvert.SerializeObject(isServiceWorking);

            Assert.AreEqual(ServiceState.Stopped, serviceStatus);
        }
        [TestMethod]
        public void TestStartService()
        {
            string serviceName = "TestService";
            ServiceInstaller.StartService(serviceName);
            ServiceState serviceStatus = ServiceInstaller.GetServiceStatus(serviceName);
            Assert.AreEqual(ServiceState.Running, serviceStatus);

        }
    }
}
