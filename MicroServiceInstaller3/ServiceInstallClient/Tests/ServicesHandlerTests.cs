using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
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
            string fileName = @"C:\Users\IEUser\Downloads\New folder\TestService.exe";
            ServiceInstaller.InstallAndStart(serviceName, displayName, fileName);
            
        }

        [TestMethod]
        public void TestInstallService2()
        {
            string serviceName = "Installer";
            string displayName = "Installer";
            string fileName = @"C:\Users\IEUser\Downloads\Installer.exe";
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
            string serviceName = "WatchdogService";
            ServiceInstaller.StartService(serviceName);
            ServiceState serviceStatus = ServiceInstaller.GetServiceStatus(serviceName);
            Assert.AreEqual(ServiceState.Running, serviceStatus);

        }

        [TestMethod]
        public void TestStartService2()
        {
            string serviceName = "Installer";
            ServiceInstaller.StartService(serviceName);
            ServiceState serviceStatus = ServiceInstaller.GetServiceStatus(serviceName);
            Assert.AreEqual(ServiceState.Running, serviceStatus);

        }

        [TestMethod]
        public void AddResources()
        {
            //string finalLocation = "c:/Downloads/Debug.zip";
            //string service = "ServiceZip";
            //MainWindow.Resources.Add(service, finalLocation);
            //Uri uri = new Uri("Resources/final.zip", UriKind.Relative);
            //StreamResourceInfo info = Application.GetContentStream(uri);
            //System.Windows.Markup.XamlReader reader = new System.Windows.Markup.XamlReader();
            //ResourceDictionary myResourceDictionary =
            //                               (ResourceDictionary)reader.LoadAsync(info.Stream);
            //Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
        }
            
    }
}
