using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MicroServiceInstaller3
{

    [TestClass]
    public class Tests
    {

        [TestMethod]
        public void St()
        {
            int c = 3 + 5;
            Assert.IsTrue(c == 9, "Problem");
        }

        [TestMethod]
        public void CreateTestXML()
        {
            //olemasolev fail tuleb 'ra kustutada ja siis luua uus
            string temporaryFolderPath = System.IO.Path.GetTempPath();

            string zipPath = System.IO.Path.Combine(temporaryFolderPath, "TestFiles");
            FShandler.CreateDirectory(zipPath);
   

            string path = System.IO.Path.Combine(zipPath, "test.exe.config");
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(string "<? xml version = "1.0" encoding = "utf-8" ?>
< configuration >");
                }
            }
            else
            {
                File.Delete(path);
            }
        }


        [TestMethod]
        public void XmlSaveTest()
        {
            string path = @"C:\Downloaded_zip_files\6cb19ed4-b3d2-420b-8e97-6565eb0df8a7\Enics.WiseToSapIntegration.Shipper.vshost.exe.config";
            var doc = XDocument.Load(path);
            var elements = doc.Descendants("appSettings").Elements();

            XElement xmlAddElement = new XElement("add");
            XAttribute configValueAttribute = new XAttribute("value", "TestValue");
            XAttribute configKeyAttribute = new XAttribute("key", "TestKey");

            xmlAddElement.Add(configKeyAttribute);
            xmlAddElement.Add(configValueAttribute);


            XElement appSettingsElement = doc.Descendants("appSettings").First(); //.First()

            appSettingsElement.Add(xmlAddElement);


            doc.Save(path);

            //kontrollida, kas lisatud key on failis olemas
        }




    }
}
