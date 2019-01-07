using CommonLibary.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceInstaller3.Handlers
{
    public class ServiceFileHandler
    {

        public static void CopyResources(string installServiceDirectory, string serviceZipDirectory)
        {
            Assembly asmb = Assembly.GetExecutingAssembly();
            string[] resourceNames = asmb.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                string fileName = resourceName.Substring(33);
                if (resourceName.EndsWith(".exe") || resourceName.EndsWith(".dll"))
                {
                    Stream strm = asmb.GetManifestResourceStream(resourceName);
                    if (strm != null)
                    {
                        string serviceDirectoryFilePath = System.IO.Path.Combine(serviceZipDirectory, fileName);
                        using (var dest = File.OpenWrite(serviceDirectoryFilePath))
                        {
                            strm.CopyTo(dest); //blocks until finished
                        }
                    }
                }
                if (resourceName.EndsWith(".txt") || resourceName.EndsWith(".sfx"))
                {
                    Stream strm = asmb.GetManifestResourceStream(resourceName);
                    if (strm != null)
                    {
                        string installServiceDirectoryFilePath = System.IO.Path.Combine(installServiceDirectory, fileName);
                        using (var dest = File.OpenWrite(installServiceDirectoryFilePath))
                        {
                            strm.CopyTo(dest); //blocks until finished
                        }
                    }
                }
            }
        }

        public static string CreateServiceZip(string serviceZipDirectory, string installServiceDirectory)
        {
            string serviceFileName = "Install.7z"; // defineerib failinime
            string serviceFilePath = System.IO.Path.Combine(installServiceDirectory, serviceFileName); // defineerib faili aadressi
            if (File.Exists(serviceFilePath)) File.Delete(serviceFilePath); // kui fail eksisteerib, kustutab selle
            SevenZip.SevenZipCompressor.SetLibraryPath(@"Resources\7za.dll"); // defineerib zipimise programmifaili
            SevenZip.SevenZipCompressor compressor = new SevenZip.SevenZipCompressor(); // loob uue zipfaili
            compressor.CompressDirectory(serviceZipDirectory, serviceFilePath); // zipib teenusefailid
            return serviceFilePath; // tagastab zipfaili aadressi
        }

        public static string CreateInstallExe(string configFileName, string serviceFilePath, string sevenZipFileName, string installServiceDirectory)
        {
            Process process = new Process();// defineerib uue protsessi
            ProcessStartInfo startInfo = new ProcessStartInfo(); // defineerib protsessi k'ivitamise  andmed
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = "C:\\Windows\\System32\\cmd.exe";
            
            startInfo.Arguments = "/C copy /b " + sevenZipFileName + " + " + configFileName + " + " + serviceFilePath + " " + installServiceDirectory + "\\Installer.exe";
            process.StartInfo = startInfo;
            process.Start(); // k'ivitab protsessi
            string installerFilePath = System.IO.Path.Combine(installServiceDirectory, "Installer.exe");
            return installerFilePath;
        }

        public static string CopyExeFile(string installServiceDirectory, string logFilePath)
        {
            IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(installServiceDirectory);
            try
            {
            foreach (var item in Files)
            {
                ErrorHandler.WriteLogMessage(logFilePath, "Files: " + item);
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (File.Exists(item))
                {
                    bool endsIn = (item.EndsWith(".exe")); // kui faili asukohanimetus sisaldab j'rgmis v''rtusi
                    if (endsIn)
                    {
                        string exeFileName = System.IO.Path.GetFileName(item);
                        ErrorHandler.WriteLogMessage(logFilePath, "Found exe file: " + exeFileName);
                        string desktopFilePath = System.IO.Path.Combine(desktopPath, exeFileName);
                        if (File.Exists(desktopFilePath)) File.Delete(desktopFilePath);
                        File.Copy(item, desktopFilePath);
                        return desktopFilePath;
                    }
                }

            }
            return "";
            }
            catch (Exception error)
            {
                ErrorHandler.WriteLogMessage(logFilePath, "Files: " + error);
                throw;
            }
        }
    }
}
