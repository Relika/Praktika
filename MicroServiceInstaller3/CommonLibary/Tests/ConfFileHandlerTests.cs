using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using CommonLibary.Handlers;
using CommonLibary.Poco;
using MicroServiceInstaller3;


namespace CommonLibary
{
    [TestClass]
    public class ConfFileHandlerTests
    {

        [TestInitialize]

        [TestMethod]
        public void FindAppSettingsFile()
        {
            string folderName = "C:/Users/IEUser/Downloads/sss";
            string filePath = ConfFileHandler.FindAppSettingsFile(folderName);
            Assert.IsNotNull(filePath);
        }

        [TestMethod]
        public void FindConfSettings()
        {
            string filePath = "C:/Users/IEUser/Downloads/sss/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<AppSettingsConfig> appSettingsCollection = ConfFileHandler.FindAppSettings(filePath);
            Assert.IsNotNull(appSettingsCollection);
        }

        [TestMethod]
        public void CompareAppSettings()
        {
            string existingfFilePath = "C:/Users/IEUser/Downloads/sss/Enics.WiseToSapIntegration.Shipper.exe.config";
            string downloadedFilePath = "C:/Users/IEUser/Downloads/ddd/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = ConfFileHandler.CompareAppSettings(existingfFilePath, downloadedFilePath);
            Assert.IsNotNull(comparedAppSettingsCollection);
        }
        [TestMethod]
        public void CompareConnectionStringsAndWriteToFile()
        {
            string existingfFilePath = "C:/Users/IEUser/Downloads/sss/Enics.WiseToSapIntegration.Shipper.exe.config";
            string downloadedFilePath = "C:/Users/IEUser/Downloads/ddd/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<ConnectionStrings> comparedConnectionStringCollection = ConfFileHandler.CompareConnectionStrings(existingfFilePath, downloadedFilePath);
            Dictionary<string, ConnectionStrings> comparedConnectionstringsDicitionary = ConfFileHandler.CreateComparedConnectionStringsDicitionary(comparedConnectionStringCollection);
            ConfFileHandler.WriteConnectionStringstoConFile(existingfFilePath, comparedConnectionstringsDicitionary);
            // kontrollin, kas andmed on molemas failis samad

            ObservableCollection<ConnectionStrings> writedCollection = ConfFileHandler.FindConnectionsStrings(existingfFilePath);
            ObservableCollection<ConnectionStrings> downloadedCollection = ConfFileHandler.FindConnectionsStrings(downloadedFilePath);

            string serializedChainWrited = JsonConvert.SerializeObject(writedCollection);
            string serializedChainDownloaded = JsonConvert.SerializeObject(downloadedCollection);

            Assert.AreEqual(serializedChainWrited, serializedChainDownloaded);
        }

        [TestMethod]
        public void CompareAppSettingsAndWriteToFile()
        {
            string existingfFilePath = "C:/Users/IEUser/Downloads/sss/Enics.WiseToSapIntegration.Shipper.exe.config";
            string downloadedFilePath = "C:/Users/IEUser/Downloads/ddd/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = ConfFileHandler.CompareAppSettings(existingfFilePath, downloadedFilePath);
            Dictionary<string, AppSettingsConfig> comparedAppSettingsDicitionary = ConfFileHandler.CreateAppSettingsDicitionary(comparedAppSettingsCollection);
            ConfFileHandler.WriteSettingsToConfFile(existingfFilePath, comparedAppSettingsDicitionary);
            // kontrollin, kas andmed on molemas failis samad

            ObservableCollection<AppSettingsConfig> writedCollection = ConfFileHandler.FindAppSettings(existingfFilePath);
            ObservableCollection<AppSettingsConfig> downloadedCollection = ConfFileHandler.FindAppSettings(downloadedFilePath);

            string serializedChainWrited = JsonConvert.SerializeObject(writedCollection);
            string serializedChainDownloaded = JsonConvert.SerializeObject(downloadedCollection);

            Assert.AreEqual(serializedChainWrited, serializedChainDownloaded);
        }

        [TestMethod]
        public void TestGetServiceName()
        {
            string fileName = Path.GetFileName("C:/Users/IEUser/Downloads/ddd/Enics.WiseToSapIntegration.Shipper.exe.config");
            string serviceName = ConfFileHandler.GetServiceName(fileName);
            string serviceNamestart = JsonConvert.SerializeObject("WiseToSapIntegration");
            string serviceNameGet = JsonConvert.SerializeObject(serviceName);

            Assert.AreEqual(serviceNamestart, serviceNameGet);
        }



       


    }
}
