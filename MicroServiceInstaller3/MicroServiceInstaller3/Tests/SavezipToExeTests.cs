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


    }
}
