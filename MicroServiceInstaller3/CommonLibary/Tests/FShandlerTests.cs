using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonLibary.Handlers;

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
            ZipFile.CreateFromDirectory(directory, zipFile);
         
            Assert.IsNotNull("C:\\Users\\User\\AppData\\Local\\Temp\\TestZip");
        }

        [TestMethod]
        public void DirectoryCopy()
        {
            string sourceDirName = "C:\\Users\\IEUser\\Downloads\\ddd";
            string destDirName = "C:\\Users\\IEUser\\Downloads\\sss";
            bool copySubDirs = true;
            FShandler.DirectoryCopy(sourceDirName, destDirName, copySubDirs);
            Assert.IsNotNull(destDirName);
        }

        [TestMethod]
        public void CreateMetaDataFile()
        {
            string selectedPath = "C:\\Users\\IEUser\\AppData\\Local\\Temp\\TestFiles";
            string workFilesFolderPath = "C:\\Users\\IEUser\\AppData\\Local\\Temp\\TestFiles";
            FShandler.CreateMetaDataFile(selectedPath, workFilesFolderPath);
            Assert.IsNotNull(workFilesFolderPath);
        }




    }
}
