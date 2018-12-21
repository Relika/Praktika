using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using CommonLibary;
using CommonLibary.Handlers;

namespace ResourceInsectionTest
{
    class Program
    {
        static void Main(string[] args)
        {

            string serviceFilePath = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService\Install.7z";
            string confFilePath = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService\config.txt";
            string sevenZipFilPath = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService\7zS.sfx";
            string exeFileDirectory = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\MicroServiceInstaller3\bin\Debug\InstallService";
            //System.Diagnostics.Process process = new System.Diagnostics.Process();
            //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            string[] files = new string[] { serviceFilePath, sevenZipFilPath, confFilePath };
            Stream streamFiles = File.Create(exeFileDirectory+"\\Install.exe");
            foreach (var item in files)
            {
                Stream i = File.OpenRead(item);
                i.CopyTo(streamFiles);
                i.Close();
            }
            //startInfo.FileName = "cmd.exe";
            //startInfo.Arguments = "/C copy /b " + confFilePath + " + " + serviceFilePath + " + " + sevenZipFilPath + " " + exeFileDirectory + "\\Installer.exe";
            //startInfo.UseShellExecute = false;
            //process.StartInfo = startInfo;
            //process.Start();



            //Console.ReadLine();

        }



        //public static MemoryStream GetResource(string path, string resourceName)
        //{
        //    var definition =
        //        AssemblyDefinition.ReadAssembly(path);

        //    foreach (var resource in definition.MainModule.Resources)
        //    {
        //        if (resource.Name == resourceName)
        //        {
        //        var embeddedResource = (EmbeddedResource)resource;
        //        var stream = embeddedResource.GetResourceStream();

        //        var bytes = new byte[stream.Length];
        //        stream.Read(bytes, 0, bytes.Length);

        //        var memStream = new MemoryStream();
        //        memStream.Write(bytes, 0, bytes.Length);
        //        memStream.Position = 0;
        //        return memStream;
        //        }
        //    }
        //    return null;
        //}

        //public static void AddResource(string path, string resourceName, byte[] resource)
        //{
        //    var definition =
        //        AssemblyDefinition.ReadAssembly(path);

        //    var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
        //    definition.MainModule.Resources.Add(er);
        //    definition.Write("start.exe");
        //}


    }
}
