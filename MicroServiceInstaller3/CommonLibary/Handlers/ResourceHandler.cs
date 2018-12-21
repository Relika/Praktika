﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace CommonLibary.Handlers
{
    public class ResourceHandler
    {
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

        public static void AddResource(string path, string resourceName, byte[] resource, string fileName)
        {
            var definition =
                AssemblyDefinition.ReadAssembly(path);

            var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
            definition.MainModule.Resources.Add(er);
            definition.Write(fileName);
        }

    }
}