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

        //MakeRandomDirectorytoTemp- ei ole m]tet testida.

        [TestMethod]
        public void TestMakeDirectorytoTemp()
        {
            string directory = "test";
            string directoryPath = FShandler.MakeDirectorytoTemp(directory);
            Assert.AreEqual(Path.Combine(Path.GetTempPath(), directory), directoryPath);
        }

        [TestMethod]
        public void MakeDirectory()
        {
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TestFiles");
            string zipDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TestZip");
            string zipFile = System.IO.Path.Combine(zipDirectory, "TestZipFile.zip");
            FShandler.MakeDirectory(zipDirectory);
            using (TransactionScope scope = new TransactionScope())
            {
                ZipFile.CreateFromDirectory(directory, zipFile);
            }
            Assert.IsNotNull("C:\\Users\\User\\AppData\\Local\\Temp\\TestZip");
        }

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




    }
}
