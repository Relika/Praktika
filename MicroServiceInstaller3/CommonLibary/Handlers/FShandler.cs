using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using CommonLibary.Poco;
using CommonLibary;

namespace CommonLibary.Handlers
{
    public class FShandler
    
    {
        /// <summary>
        /// Selects folder
        /// </summary>
        /// <param name="folderBrowserDialog1">Selected folder</param>
        /// <param name="selectedFolderLabel">Label where selected folder are previewed for user</param>
        /// <param name="savebutton">Zip file button</param>
        /// <returns>Returns selected folder path</returns>
        public static string ChooseFolder(FolderBrowserDialog folderBrowserDialog1, System.Windows.Controls.Label selectedFolderLabel, System.Windows.Controls.Button savebutton)
        {
            string selectedPath = folderBrowserDialog1.SelectedPath; 
            selectedFolderLabel.Content = selectedPath;
            if (selectedFolderLabel.HasContent)
            {
                savebutton.IsEnabled = true;
            }
            return selectedPath;
        }
        /// <summary>
        /// Makes directory with random name to temp folder
        /// </summary>
        /// <returns>Returns random directory path</returns>
        public static string MakeRandomDirectorytoTemp()
        {
            string directoryName = Guid.NewGuid().ToString();
            string directoryPath = MakeDirectorytoTemp(directoryName);
            return directoryPath;
        }
        /// <summary>
        /// Makes directory with specified name to temp folder
        /// </summary>
        /// <param name="directory">Specified directory name</param>
        /// <returns>Returns specified directory path</returns>
        public static string MakeDirectorytoTemp(string directory)
        {
            string folderPath = System.IO.Path.GetTempPath();
            string path = System.IO.Path.Combine(folderPath, directory);
            MakeDirectory(path);
            return path;
        }
        /// <summary>
        /// Makes directory
        /// </summary>
        /// <param name="fullPath">Specified directory full path</param>
        public static void MakeDirectory (string fullPath)
        {
            // If the destination directory doesn't exist, create it.
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            Directory.CreateDirectory(fullPath);
        }
        /// <summary>
        /// Copies directories and subdirectories
        /// </summary>
        /// <param name="sourceDirName">Source directory name</param>
        /// <param name="destDirName">Destination directory name</param>
        /// <param name="copySubDirs">Boolean true= copy subdirectories, false= don´t copy subdirectories</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }
            DirectoryInfo[] dirs = dir.GetDirectories();
            MakeDirectory(destDirName);
            // Gets the files from the directory and copies them to the new location.
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
        /// <summary>
        /// Creates metadata file to specified directory and writes selected path in it.
        /// </summary>
        /// <param name="selectedPath">Selected path</param>
        /// <param name="workFilesFolderPath">Specified directory</param>
        public static void CreateMetaDataFile(string selectedPath, string workFilesFolderPath)
        {
            string RandomFileName = Guid.NewGuid().ToString();
            string path = System.IO.Path.Combine(workFilesFolderPath, RandomFileName + ".txt");
            if (!File.Exists(path))
            {
                // Create a file and write in it.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(selectedPath);
                }
            }
        }
        /// <summary>
        /// Creates log file to specified directory
        /// </summary>
        /// <param name="selectedDirectory">Specified directory</param>
        /// <returns>Returns log file path</returns>
        public static string CreateLogFile (string selectedDirectory)
        {
            string logFileName = "log.txt";
            string logFilePath = System.IO.Path.Combine(selectedDirectory, logFileName);
            FileInfo fi = new FileInfo(logFilePath);
            // Actually create the file.
            FileStream fs = fi.Create();
            // Modify the file as required, and then close the file.
            fs.Close();
            return logFilePath;
        }
        /// <summary>
        /// Copies Zipfile from program resources to folder
        /// </summary>
        /// <param name="zipArchive">Zipfile</param>
        /// <returns>Returns extract folder path</returns>
        public static string CopyResourcesToTemporayFolder(ZipArchive zipArchive)
        {
            string extractFolderPath = FShandler.MakeRandomDirectorytoTemp();
            foreach (var item in zipArchive.Entries)
            {
                if (item.IsFolder())
                {
                    if (!Directory.Exists(Path.Combine(extractFolderPath, item.FullName)))
                    {
                        Directory.CreateDirectory(Path.Combine(extractFolderPath, item.FullName));
                    }
                }
                else
                {
                    string destinationPath = Path.GetFullPath(Path.Combine(extractFolderPath, item.FullName));
                    // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                    // are case-insensitive.
                    if (destinationPath.StartsWith(extractFolderPath, StringComparison.Ordinal))
                        item.ExtractToFile(destinationPath);
                }
            }
            return extractFolderPath;
        }
        /// <summary>
        /// Gets current program location
        /// </summary>
        /// <returns>Returns current program location path</returns>
        public static string GetProgramLocation()
        {
            string location = System.Reflection.Assembly.GetEntryAssembly().Location;
            string directoryPath = Path.GetDirectoryName(location);
            return directoryPath;
        }
    }
}
