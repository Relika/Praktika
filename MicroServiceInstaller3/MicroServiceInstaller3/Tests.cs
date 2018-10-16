﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.IO.Compression;
using System.Collections.ObjectModel;
using MicroServiceInstaller3.Poco;

namespace MicroServiceInstaller3
{

    [TestClass]
    public class Tests
    {      
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
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("connectionStrings");
                xmlWriter.WriteStartElement("add");
                xmlWriter.WriteAttributeString("name", "LogBook");
                xmlWriter.WriteAttributeString("connectionString","lvh8tgeugeo8496u49tjft0p358990u" );
                xmlWriter.WriteAttributeString("providerName","SqlServer" );

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




        //[TestMethod]
        public XElement   AddXmlElements(string path) //XElement
        {
            //string path = System.IO.Path.Combine(Path.GetTempPath(), "TestFiles", "test.exe.config");
            string xmlElement = "connectionStrings"; // "connectionStrings" "appSettings"
            List<KeyValuePair<string, string>> appSettingList = CreateAppSettingsList();

            var doc = XDocument.Load(path);
            var elements = doc.Descendants(xmlElement).Elements();

            XElement xmlAddElement = new XElement("add");
            foreach (var appSetting in appSettingList)
            {
                XName name = appSetting.Key;
                string value = appSetting.Value;
                XAttribute configAttribute = new XAttribute(name, value);
                xmlAddElement.Add(configAttribute);
            }
            XElement appSettingsElement = doc.Descendants(xmlElement).First(); //.First()

            appSettingsElement.Add(xmlAddElement);
            doc.Save(path);

            return xmlAddElement;
        }

        private static List<KeyValuePair<string, string>> CreateAppSettingsList()
        {
            String[] names = new string[] { "name", "connectionString", "providerName" }; // "name", "connectionString", "providerName" "key", "value"
            String[] values = new string[] { "SeeOnNimi", "seeOnString", "seeonproviderName" }; // "SeeOnNimi", "seeOnString", "seeonproviderName"   "SeeOnKey", "SeeOnValue"

            var appSettingList = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < names.Length; i++)
            {
                appSettingList.Add(new KeyValuePair<string, string>(names[i], values[i]));
            }

            return appSettingList;
        }

        private static List<KeyValuePair<string, string>> CreateAppSettingsListFalse()
        {
            String[] names = new string[] { "name", "connectionString", "providerName" }; // "name", "connectionString", "providerName" "key", "value"
            String[] values = new string[] { "SeeEiOleNimi", "seeEiOleString", "seeEiOleproviderName" }; // "SeeOnNimi", "seeOnString", "seeonproviderName"   "SeeOnKey", "SeeOnValue"

            var appSettingList = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < names.Length; i++)
            {
                appSettingList.Add(new KeyValuePair<string, string>(names[i], values[i]));
            }

            return appSettingList;
        }

        public bool DoesXmlElementExist(XElement xmlElement, string path)
        {
            string xmlElementName = "connectionStrings"; // "connectionStrings" "appSettings"
            List<KeyValuePair<string, string>> appSettingList = CreateAppSettingsListFalse();

            var doc = XDocument.Load(path);
            var elements = doc.Descendants(xmlElementName).Elements();

            foreach (var appSetting in appSettingList) // siin on yks key + value paar ehk atribute
            {
                bool attributeFound = false;
                foreach (var item in elements) // siin on terve rida, mida kontrollib 
                {
                    if (item.Attribute(appSetting.Key) != null && item.Attribute(appSetting.Key).Value == appSetting.Value) // kas selles reas on sellise nime ja v''rtusega atribute.
                    {
                        // kui atribute on olemas, on mul vaja teada, kas selles reas on olemas ka teised atribute-d
                        attributeFound = true;
                        break;
                    }
                    // sellise nime ja v''rtusega v]i sellise v''rtusega attribute ei ole olemas;
                    return false;
                }
                if(attributeFound) continue; // selline atribute on olemas, siis l'heb kontrollib teised ka
            }
            // k]ik elemendid on kontrollitud ja sobivad
            return true;
        }

        [TestMethod]
        public void XmlSaveTest()
        {
            string path = CreateTestXML();
            XElement xmlAddElement =  AddXmlElements(path);
            bool elementExists = DoesXmlElementExist(xmlAddElement, path);
            Assert.IsTrue(elementExists);
        }

        [TestMethod]
        public void CreateTestZipFile()
        {
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TestFiles");
            string zipDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TestZip");
            string zipFile = System.IO.Path.Combine(zipDirectory, "TestZipFile.zip");
            FShandler.CreateDirectory(zipDirectory);
            using (TransactionScope scope = new TransactionScope())
            {
                ZipFile.CreateFromDirectory(directory, zipFile);
            }

        }
        [TestMethod]
        public void TestFindConnectionsStrings()
        {
            //ObservableCollection<ConnectionStrings> ConnectionStringsCollection = new ObservableCollection<ConnectionStrings>();
            //try
            //{
                string fileSystemEntry = "C:\\Users\\User\\Downloads\\test67";
                ObservableCollection<ConnectionStrings> connectionStringsCollection = Conffilehandler.FindConnectionsStrings(fileSystemEntry);

                //var doc = XDocument.Load(fileSystemEntry);
                //var elements = doc.Descendants("connectionStrings").Elements();

                //foreach (var element in elements)
                //{
                //    ConnectionStrings connectionStrings = new ConnectionStrings();
                //    connectionStrings.Name = (string)element.Attribute("name");
                //    connectionStrings.ConnectionString = (string)element.Attribute("connectionString");
                //    connectionStrings.ProviderName = (string)element.Attribute("providerName");
                //    ConnectionStringsCollection.Add(connectionStrings);
                //}
            //}
            //catch (Exception error)
            //{
                //statusLabel.Content = error.Message;
                //statusLabel.Content = "This file does not consist connectionSettings, please select another file";
                Assert.IsNotNull(connectionStringsCollection);
            //}
            ////Assert
            //return ConnectionStringsCollection;
        }

        // [TestMethod]

        //public static Dictionary<string, MainWindow.ConnectionStrings> CreateConnectionStringsDicitionary()
        //{

        //}
    }
}
