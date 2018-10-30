using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using MicroServiceInstaller3.Poco;

namespace MicroServiceInstaller3
{
    [TestClass]
    public class ConfFileHandlerTests
    {

        [TestInitialize]

        [TestMethod]
        public void FindAppSettingsFile()
        {
            string folderName = "C:/Users/User/Downloads/eye";
            string filePath = ConfFileHandler.FindAppSettingsFile(folderName);
            Assert.IsNotNull(filePath);
        }

        [TestMethod]
        public void FindConfSettings()
        {
            string filePath = "C:/Users/User/Downloads/eye/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<Poco.AppSettingsConfig> appSettingsCollection = ConfFileHandler.FindAppSettings(filePath);
            Assert.IsNotNull(appSettingsCollection);
        }

        [TestMethod]
        public void CompareAppSettings ()
        {
            string existingfFilePath = "C:/Users/User/Downloads/eye/Enics.WiseToSapIntegration.Shipper.exe.config";
            string downloadedFilePath = "C:/Users/User/Downloads/dddd/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<Poco.AppSettingsConfig> comparedAppSettingsCollection = ConfFileHandler.CompareAppSettings(existingfFilePath, downloadedFilePath);
            Assert.IsNotNull(comparedAppSettingsCollection);
        }
        [TestMethod]
        public void CompareConnectionStringsAndWriteToFile()
        {
            string existingfFilePath = "C:/Users/User/Downloads/eye/Enics.WiseToSapIntegration.Shipper.exe.config";
            string downloadedFilePath = "C:/Users/User/Downloads/dddd/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<Poco.ConnectionStrings> comparedConnectionStringCollection = ConfFileHandler.CompareConnectionStrings(existingfFilePath, downloadedFilePath);
            Dictionary<string, Poco.ConnectionStrings> comparedConnectionstringsDicitionary = ConfFileHandler.CreateComparedConnectionStringsDicitionary (comparedConnectionStringCollection);
            ConfFileHandler.WriteConnectionStringstoConFile(existingfFilePath, comparedConnectionstringsDicitionary);
            // kontrollin, kas andmed on molemas failis samad
            
            ObservableCollection<Poco.ConnectionStrings> writedCollection = ConfFileHandler.FindConnectionsStrings(existingfFilePath);
            ObservableCollection<Poco.ConnectionStrings> downloadedCollection = ConfFileHandler.FindConnectionsStrings(downloadedFilePath);

            string serializedChainWrited = JsonConvert.SerializeObject(writedCollection);
            string serializedChainDownloaded = JsonConvert.SerializeObject(downloadedCollection);

            Assert.AreEqual(serializedChainWrited, serializedChainDownloaded);
        }

        [TestMethod]
        public void CompareAppSettingsAndWriteToFile()
        {
            string existingfFilePath = "C:/Users/User/Downloads/eye/Enics.WiseToSapIntegration.Shipper.exe.config";
            string downloadedFilePath = "C:/Users/User/Downloads/dddd/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<Poco.AppSettingsConfig> comparedAppSettingsCollection = ConfFileHandler.CompareAppSettings(existingfFilePath, downloadedFilePath);
            Dictionary<string, Poco.AppSettingsConfig> comparedAppSettingsDicitionary = ConfFileHandler.CreateComparedAppSettingsDicitionary(comparedAppSettingsCollection);
            ConfFileHandler.WriteSettingsToConfFile(existingfFilePath, comparedAppSettingsDicitionary);
            // kontrollin, kas andmed on molemas failis samad

            ObservableCollection<Poco.AppSettingsConfig> writedCollection = ConfFileHandler.FindAppSettings(existingfFilePath);
            ObservableCollection<Poco.AppSettingsConfig> downloadedCollection = ConfFileHandler.FindAppSettings(downloadedFilePath);

            string serializedChainWrited = JsonConvert.SerializeObject(writedCollection);
            string serializedChainDownloaded = JsonConvert.SerializeObject(downloadedCollection);

            Assert.AreEqual(serializedChainWrited, serializedChainDownloaded);
        }

        [TestMethod]
        public void TestGetServiceName()
        {
            string fileName = Path.GetFileName("C:/Users/User/Downloads/dddd/Enics.WiseToSapIntegration.Shipper.exe.config");
            string serviceName = ConfFileHandler.GetServiceName(fileName);
            string serviceNamestart = JsonConvert.SerializeObject("WiseToSapIntegration");
            string serviceNameGet = JsonConvert.SerializeObject(serviceName);

            Assert.AreEqual(serviceNamestart, serviceNameGet);
        }



        [TestMethod]
        public void TestInstallService()
        {
            string serviceName = "TestService";
            string displayName = "TestService";
            string fileName = "C:/Users/User/source/repos/Praktika/MicroServiceInstaller3/TestService/bin/Debug/TestService.exe";
            Poco.ServiceInstaller.InstallAndStart(serviceName, displayName, fileName);
        }

        [TestMethod]
        public void TestStopService()
        {
            string serviceName = "TestService";
            Poco.ServiceInstaller.StopService(serviceName);
            ServiceState serviceStatus= Poco.ServiceInstaller.GetServiceStatus(serviceName);
            //string expectedValue = JsonConvert.SerializeObject("1");
            //string actualValue = JsonConvert.SerializeObject(isServiceWorking);

            Assert.AreEqual(ServiceState.Stopped, serviceStatus);
        }



    }
}
