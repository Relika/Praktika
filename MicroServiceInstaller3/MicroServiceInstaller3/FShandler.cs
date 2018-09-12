﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MicroServiceInstaller3
{
    public class FShandler
    
    {

        /// <summary>
        /// Creates directory and if this directory exists, deletes directory and its subdirectories and files.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static DirectoryInfo CreateDirectory (string fullPath)
        {
            // lisada kontroll, kas sisestatud fullPathiga on v[imalik kausta teha.
            DirectoryInfo createdDirectory = null;
            // If the destination directory doesn't exist, create it.
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }

            createdDirectory = Directory.CreateDirectory(fullPath);
            return createdDirectory;
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
            FShandler.CreateDirectory(destDirName);

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

    }
}