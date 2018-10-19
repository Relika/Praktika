using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroServiceInstaller3
{
    [TestClass]
    public class ConfFileHandlerTests
    {

        [TestMethod]
        public void FindAppSettingsFile()
        {
            string folderName = "C:/Users/User/Downloads/Test56";
            string filePath = ConfFileHandler.FindAppSettingsFile(folderName);
            Assert.IsNotNull(filePath);
        }

        [TestMethod]
        public void FindConfSettings()
        {
            string filePath = "C:/Users/User/Downloads/Test56/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<Poco.AppSettingsConfig> appSettingsCollection = ConfFileHandler.FindAppSettings(filePath);
            Assert.IsNotNull(appSettingsCollection);
        }

        [TestMethod]
        public void CompareAppSettings ()
        {
            string existingfFilePath = "C:/Users/User/Downloads/Test56/Enics.WiseToSapIntegration.Shipper.exe.config";
            string downloadedFilePath = "C:/Users/User/Downloads/Test100/Enics.WiseToSapIntegration.Shipper.exe.config";
            ObservableCollection<Poco.AppSettingsConfig> comparedAppSettingsCollection = ConfFileHandler.CompareAppSettings(existingfFilePath, downloadedFilePath);
            Assert.IsNotNull(comparedAppSettingsCollection);
        }

    }
}
