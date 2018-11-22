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
            // string exeFilePath = @"C:\Users\IEUser\source\repos\Relika\Praktika\Praktika\MicroServiceInstaller3\ResourceInsectionTest\ServiceInstallClient.exe";
            string exeFilePath = @"ServiceInstallClient.exe";
            string zipPath = @"C:\FinalZipDirectory\final.zip";
            byte[] finalZipBytes = File.ReadAllBytes(zipPath);
            //byte[] array = new byte[] { 1, 1, 0, 34 };

            AddResource(exeFilePath, "Debug1.zip", finalZipBytes);
            MemoryStream memoryStream = GetResource("start.exe", "Debug1.zip");
            ZipArchive zipArchive = new ZipArchive(memoryStream);
            string temporaryFolder = FShandler.CopyResourcesToTemporayFolder(zipArchive);


        }



        public static MemoryStream GetResource(string path, string resourceName)
        {
            var definition =
                AssemblyDefinition.ReadAssembly(path);

            foreach (var resource in definition.MainModule.Resources)
            {
                if (resource.Name == resourceName)
                {
                var embeddedResource = (EmbeddedResource)resource;
                var stream = embeddedResource.GetResourceStream();

                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                var memStream = new MemoryStream();
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Position = 0;
                return memStream;
                }
            }
            return null;
        }

        public static void AddResource(string path, string resourceName, byte[] resource)
        {
            var definition =
                AssemblyDefinition.ReadAssembly(path);

            var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
            definition.MainModule.Resources.Add(er);
            definition.Write("start.exe");
        }


    }
}
