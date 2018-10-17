using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroServiceInstaller3
{

    [TestClass]
    public class FShandlerTests
    {
        [TestMethod]
        public void CreateDirectory()
        {
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TestFiles");
            string zipDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TestZip");
            string zipFile = System.IO.Path.Combine(zipDirectory, "TestZipFile.zip");
            FShandler.CreateDirectory(zipDirectory);
            using (TransactionScope scope = new TransactionScope())
            {
                ZipFile.CreateFromDirectory(directory, zipFile);
            }
            Assert.IsNotNull("C:\\Users\\User\\AppData\\Local\\Temp\\TestZip");

        }


        //[TestMethod]
        //public void CopyAll()
        //{
        //    DirectoryInfo source = new DirectoryInfo("C:\\Users\\User\\Downloads\\Test56");
        //    DirectoryInfo target = new DirectoryInfo("C:\\Users\\User\\Downloads\\test67");
        //    FShandler.CopyAll(source, target);
        //    Assert.IsNotNull(target);
        //}

        [TestMethod]
        public void DirectoryCopy()
        {
            string sourceDirName = "C:\\Users\\User\\Downloads\\Test56";
            string destDirName = "C:\\Users\\User\\Downloads\\Test100";
            bool copySubDirs = true;
            FShandler.DirectoryCopy(sourceDirName, destDirName, copySubDirs);
            Assert.IsNotNull(destDirName);
        }

        [TestMethod]
        public void CreateMetaDataFile()
        {
            string selectedPath = "C:\\Users\\User\\AppData\\Local\\Temp\\TestFiles";
            string workFilesFolderPath = "C:\\Users\\User\\AppData\\Local\\Temp\\TestFiles";
            FShandler.CreateMetaDataFile(selectedPath, workFilesFolderPath);
            Assert.IsNotNull(workFilesFolderPath);
        }

        [TestMethod]
        public void TestMakeFolders()
        {
            string directory = "test";
            string folderPath = System.IO.Path.GetTempPath();
            string path = System.IO.Path.Combine(folderPath, directory);
            FShandler.CreateDirectory(path);
            Assert.AreEqual("C:\\Users\\User\\AppData\\Local\\Temp\\test", path);
            
        }

        [TestMethod]
        public void TestMakeFolder()
        {
            string path = "C:\\FinalzipDirectory";
            FShandler.CreateDirectory(path);
            //Kas saab kontrollida kasuta loomist?
        }


    }
}
