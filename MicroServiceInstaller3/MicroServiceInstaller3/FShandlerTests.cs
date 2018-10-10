using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroServiceInstaller3
{

    [TestClass]
    public class FShandlerTests
    {
        [TestMethod]
        public void CopyAll()
        {
            DirectoryInfo source = new DirectoryInfo("C:\\Users\\User\\Downloads\\Test56");
            DirectoryInfo target = new DirectoryInfo("C:\\Users\\User\\Downloads\\test67");
            CopyAll(source, target);
        }

        public void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
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

        [TestMethod]
        public void CreateMetaDataFile()
        {
            string selectedPath = "C:\\Users\\User\\AppData\\Local\\Temp\\TestFiles";
            string workFilesFolderPath = "C:\\Users\\User\\AppData\\Local\\Temp\\TestFiles";
            string RandomFileName = Guid.NewGuid().ToString();

            string path = System.IO.Path.Combine(workFilesFolderPath, RandomFileName + ".txt");
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(selectedPath);
                }
            }
        }
    }
}
