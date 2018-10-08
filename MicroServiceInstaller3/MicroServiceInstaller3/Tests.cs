using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

        
        public string CreateTestXML()
        {
            //olemasolev fail tuleb 'ra kustutada ja siis luua uus
            string temporaryFolderPath = System.IO.Path.GetTempPath();

            string testXmlDirectory = System.IO.Path.Combine(temporaryFolderPath, "TestFiles");
            FShandler.CreateDirectory(testXmlDirectory);
   
            string testXmlPath = System.IO.Path.Combine(testXmlDirectory, "test.exe.config");
            if (!File.Exists(testXmlPath))
            {
                XmlWriter xmlWriter = XmlWriter.Create(testXmlPath);

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("configuration");

                xmlWriter.WriteStartElement("appSettings");

                xmlWriter.WriteStartElement("add");
              //  xmlWriter.WriteAttributeString("key", "Wisesite");
                xmlWriter.WriteAttributeString("value", "SE_30");

                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
            else
            {
                File.Delete(testXmlPath);
            }
            return testXmlPath;
        }

        public XElement AddXmlElement(string path)
        {
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

            return xmlAddElement;
        }

        public bool DoesXmlElementExist(XElement xmlElement, string path)
        {
            var doc = XDocument.Load(path);
            var elements = doc.Descendants("appSettings").Elements();

            foreach (var item in elements)
            {
                //string keyAttributeValue;
                //string valueAttributeValue;

                if (item.Attribute("key") != null)   //item.Attributes("key") != null
                {
                    if (item.Attribute("key").Value == xmlElement.Attribute("key").Value)
                    {
                        if (item.Attributes("value") != null)
                        {
                            if (item.Attribute("value").Value == xmlElement.Attribute("value").Value)
                            {
                                return true;
                            }
                            continue;
                        }
                        continue;
                    }
                    continue;
                }
                continue;
                //XAttribute keyAttribute = item.Attribute("key");
                //keyAttributeValue = keyAttribute.Value;
                //{
                //    XAttribute valueAttribute = item.Attribute("value");
                //    valueAttributeValue = valueAttribute.Value;
                //}

            }
            return false;
        }

        [TestMethod]
        public void XmlSaveTest()
        {

            string path = CreateTestXML();
            XElement xmlAddElement =  AddXmlElement(path);
            bool elementExists = DoesXmlElementExist(xmlAddElement, path);


            //kontrollida, kas lisatud key on failis olemas

        }




    }
}
