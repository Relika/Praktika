using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibary.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceInstallClient.Tests
{
    [TestClass]
    public class SavezipToExeTests
    {

        [TestMethod]
        public void SaveZipToExe()
        {
            string finalZipFileName = "final.zip";
            string finalZipFilePath = @"C:\FinalZipDirectory\final.zip";
            byte[] finalZipBytes = File.ReadAllBytes(finalZipFilePath);
            string exeFilePath = @"ServiceInstallClient.exe";
            string finalExe = "start.exe";
            ResourceHandler.AddResource(exeFilePath, finalZipFileName, finalZipBytes, finalExe);
            MemoryStream memoryStream = ResourceHandler.GetResource("start.exe", "final.zip");
            ZipArchive zipArchive = new ZipArchive(memoryStream);
            string temporaryFolder = FShandler.CopyResourcesToTemporayFolder(zipArchive);
        }

        [TestMethod]
        public void TestCreateServiceZip()
        {
            string temporaryDirectory = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\Template";
            string installServiceDirectory = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService";
            MicroServiceInstaller3.MainWindow.CreateServiceZip(temporaryDirectory, installServiceDirectory);
            Assert.IsNotNull(ConfFileHandler.FindZipFile(installServiceDirectory));


        }

        [TestMethod]
        public void TestGettxtFiles()
        {
            //MicroServiceInstaller3.MainWindow.Listresources();
            //string[] txtfiles = MicroServiceInstaller3.MainWindow.GetAllTxt();
        }
        [TestMethod]
        public void TestCopyResourses()
        {
            string serviceZipDirectory = FShandler.MakeDirectorytoTemp("test1");
            string installDirectory = FShandler.MakeDirectorytoTemp("test2");
            MicroServiceInstaller3.MainWindow.CopyResources(serviceZipDirectory, installDirectory);
        }

        [TestMethod]
        public void TestCopyResources1()
        {
            string installServiceDirectory = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService";
            string confFilePath = Path.Combine(installServiceDirectory, "config.txt");
            string sevenZipFilePath = Path.Combine(installServiceDirectory, "7zS.sfx");
            MicroServiceInstaller3.MainWindow.CopyResources(confFilePath, sevenZipFilePath);
        }

        [TestMethod]
        public void TestCreateInstallExe()
        {
            string serviceFilePath = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService\Install.7z";
            string confFilePath = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService\config.txt";
            string sevenZipFilPath = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService\7zS.sfx";
            string exeFileDirectory = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService";
            MicroServiceInstaller3.MainWindow.CreateInstallExe(confFilePath, serviceFilePath, sevenZipFilPath, exeFileDirectory);
        }

        [TestMethod]
        public void TestCopyExe()
        {
            string installServiceDirectory = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService";
            MicroServiceInstaller3.MainWindow.CopyExeFile(installServiceDirectory);
        }




           



    }

}
