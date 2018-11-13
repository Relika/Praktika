﻿using System;
using System.IO;
using System.Windows.Forms;

namespace CommonLibary.Handlers
{
    public class FShandler
    
    {

        /// <summary>
        /// Creates directory and if this directory exists, deletes directory and its subdirectories and files.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string ChooseFolder(FolderBrowserDialog folderBrowserDialog1, System.Windows.Controls.Label selectedFolderLabel, System.Windows.Controls.Button savebutton)
        {
            string selectedPath = folderBrowserDialog1.SelectedPath; // Loob muutuja, mis vastab valitud kaustale
            selectedFolderLabel.Content = selectedPath;// M''rab, kuhu kuvatakse valitud kausta sisu
            if (selectedFolderLabel.HasContent)
            {
                savebutton.IsEnabled = true;
            }
            return selectedPath;
        }

        public static string MakeRandomDirectorytoTemp()
        {
            string directoryName = Guid.NewGuid().ToString();
            string directoryPath = MakeDirectorytoTemp(directoryName);
            return directoryPath;
        }

        public static string MakeDirectorytoTemp(string directory)
        {
            string folderPath = System.IO.Path.GetTempPath();
            string path = System.IO.Path.Combine(folderPath, directory);
            MakeDirectory(path);
            return path;
        }


        public static string MakeDirectory (string fullPath)
        {
            // lisada kontroll, kas sisestatud fullPathiga on v[imalik kausta teha.
            DirectoryInfo createdDirectory = null;
            // If the destination directory doesn't exist, create it.
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }

            createdDirectory = Directory.CreateDirectory(fullPath);
            return createdDirectory.ToString();
        }

      public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            //  string destDirName = LbworkFilesFolder.Content.ToString();
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            MakeDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        //string RandomFileName = "";

        public static void CreateMetaDataFile(string selectedPath, string workFilesFolderPath)
        {
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