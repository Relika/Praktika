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
        public void TestChooseFolder()
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select directory";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            string selectedPath = folderBrowserDialog1.SelectedPath; // Loob muutuja, mis vastab valitud kaustale
            //selectedFolderLabel.Content = selectedPath;// M''rab, kuhu kuvatakse valitud kausta sisu
            //if (selectedFolderLabel.HasContent)
            //{
            //    savebutton.IsEnabled = true;
            //}
            //return selectedPath;
        }

        //[TestMethod]
        //public static void CopyAll()
        //{
        //    DirectoryInfo source = new DirectoryInfo("C:\\Users\\User\\Downloads\\Test56");
        //    DirectoryInfo target = new DirectoryInfo("C:\\Users\\User\\Downloads\\test67");
        //    return CopyAll(source, target);
        //}

        //public static void CopyAll(DirectoryInfo source,DirectoryInfo target)
        //{
        //    Directory.CreateDirectory(target.FullName);

        //    // Copy each file into the new directory.
        //    foreach (FileInfo fi in source.GetFiles())
        //    {
        //        Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
        //        fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name), true);
        //    }

        //    // Copy each subdirectory using recursion.
        //    foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        //    {
        //        DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
        //        CopyAll(diSourceSubDir, nextTargetSubDir);
        //    }
        //}
    }
}
