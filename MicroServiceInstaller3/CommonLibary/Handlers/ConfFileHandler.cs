﻿using CommonLibary.Poco;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace CommonLibary.Handlers
{
    public class ConfFileHandler
    {

        public static string FindAppSettingsFile(string temporaryFolder)
        {
            foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(temporaryFolder, "*", SearchOption.AllDirectories)) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
            {
                if (!File.Exists(fileSystemEntry)) continue; // kui fail ei eksisteeri, j'tkab
                if (fileSystemEntry.EndsWith(".exe.config")) return fileSystemEntry;
            }
            return string.Empty;
        }

        public static string FindZipFile(string folder)
        {
            foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(folder, "*", SearchOption.AllDirectories)) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
            {
                if (!File.Exists(fileSystemEntry)) continue; // kui fail ei eksisteeri, j'tkab
                if (fileSystemEntry.EndsWith(".zip")) return fileSystemEntry;
            }
            return string.Empty;
        }



        public static ObservableCollection<AppSettingsConfig> FindAppSettings(string fileSystemEntry)
        {
            ObservableCollection<AppSettingsConfig> appSettingsCollection = new ObservableCollection<AppSettingsConfig>();

                var doc = XDocument.Load(fileSystemEntry);
                var elements = doc.Descendants("appSettings").Elements();

                foreach (var item in elements)
                {
                    AppSettingsConfig appSetting = new AppSettingsConfig();
                    appSetting.Key = (string)item.Attribute("key");
                    appSetting.NewValue = (string)item.Attribute("value"); // muutsin newvalue-> value
                    appSetting.IsValueNew = true;
                    appSettingsCollection.Add(appSetting);
                }
            return appSettingsCollection;
        }
        
        public static ObservableCollection<AppSettingsConfig> CompareAppSettings( string existingFileSystemEntry, string downloadedFileSystemEntry)
        {
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = new ObservableCollection<AppSettingsConfig>();

                HashSet<string> KeySet = new HashSet<string>();
                FindAppsettingKeys(existingFileSystemEntry, KeySet);
                FindAppsettingKeys(downloadedFileSystemEntry, KeySet);
                foreach (var key in KeySet)
                {
                    AppSettingsConfig appSetting = new AppSettingsConfig();
                    appSetting.Key = key;
                    string downloadedAppSettingValue = FindAppsettingValue(downloadedFileSystemEntry, key);
                    if (!string.IsNullOrEmpty(downloadedAppSettingValue))
                    {
                        appSetting.IsValueNew = true;
                        
                    }
                    string existingAppSettingValue = FindAppsettingValue(existingFileSystemEntry, key);
                    if (!string.IsNullOrEmpty(existingAppSettingValue))
                    {
                        appSetting.IsValueExist = true;
                    }
                    appSetting.NewValue = downloadedAppSettingValue;
                    appSetting.ExistingValue = existingAppSettingValue;
                    comparedAppSettingsCollection.Add(appSetting);
                }

            return comparedAppSettingsCollection;

        }

        public static Dictionary<string, AppSettingsConfig> CreateComparedAppSettingsDicitionary(ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection)
        {
            Dictionary<string, AppSettingsConfig> newAppSettingsDictionary = new Dictionary<string, AppSettingsConfig>();
            foreach (var appSetting in comparedAppSettingsCollection)
            {
                string key = appSetting.Key;
                newAppSettingsDictionary.Add(key, appSetting);
            }
            return newAppSettingsDictionary;
        }

        private static void FindAppsettingKeys(string fileSystemEntry, HashSet<string> KeySet)
        {
            var doc = XDocument.Load(fileSystemEntry);
            var elements = doc.Descendants("appSettings").Elements();
            foreach (var item in elements)
            {
                KeySet.Add((string)item.Attribute("key"));
            }
        }

        private static string FindAppsettingValue(string confFilePath, string key)
        {
            var doc = XDocument.Load(confFilePath);
            var elements = doc.Descendants("appSettings").Elements();
            foreach (var item in elements)
            {
                if ((string)item.Attribute("key") == key)
                {
                    return (string)item.Attribute("value");

                }
                else
                {
                }
            }
            return null;
        }

        public static Dictionary<string, AppSettingsConfig> ReadModifiedAppSettings( ObservableCollection<AppSettingsConfig> modifiedAppSettings)
        {
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = new Dictionary<string, AppSettingsConfig>();
            foreach (var item in modifiedAppSettings)
            {
                AppSettingsConfig appSetting = item as AppSettingsConfig;
                try
                {
                appSettingsDictionary.Add(appSetting.Key, appSetting);
                }
                catch (Exception error)
                {
                    //return error;
                }

            }
            return appSettingsDictionary;
        }

        public static void WriteSettingsToConfFile(string appConfigPath, Dictionary<string, AppSettingsConfig> appSettingsDic)
        {
            var doc = XDocument.Load(appConfigPath);
            var elements = doc.Descendants("appSettings").Elements();
            foreach (var appsettings in appSettingsDic)
            {
                string key = appsettings.Key;
                AppSettingsConfig conf = appsettings.Value;
                //Kui on  RbExisting value on true, siis otsib key ja kirjutab Existing v''rtuse faili
                if (conf.RbExistingValue == true)
                {
                    foreach (var item in elements)
                    {
                        if (appsettings.Key == (string)item.Attribute("key"))
                        {
                            item.Attribute("value").Value = conf.ExistingValue;
                            break;
                        }
                    }
                }
                //lisab uue juhul, kui existing elementi ei olnud
                else  //parandasin 0 asemele null
                {
                    if (conf.ExistingValue == null)
                    {
                        //AddKeyToConfFile(appsettings, elements, doc, conf);
                        if (conf.IsValueNew)
                        {
                            foreach (var item in elements)
                            {
                                if (appsettings.Key == (string)item.Attribute("key"))
                                {
                                    item.Attribute("value").Value = conf.NewValue;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            AddNewAppSettingtoConFile(doc, appsettings, conf);
                        }

                    }
                     else
                    {
                        foreach (var item in elements)
                        {
                            if (appsettings.Key == (string)item.Attribute("key"))
                            {
                                item.Attribute("value").Value = conf.NewValue;
                                break;
                            }
                        }
                    }                   

                }
                //Kui on  RbNewValue value on true, siis kirjutab valitud v''rtuse faili

            }
            doc.Save(appConfigPath);
        }

        public static void AddNewAppSettingtoConFile(XDocument doc, KeyValuePair<string, AppSettingsConfig> appsettings, AppSettingsConfig conf)
        {
            XElement xmlAddElement = new XElement("add");
            XAttribute configValueAttribute = new XAttribute("value", conf.NewValue.ToString());
            XAttribute configKeyAttribute = new XAttribute("key", appsettings.Key.ToString());
            xmlAddElement.Add(configKeyAttribute);
            xmlAddElement.Add(configValueAttribute);
            XElement appSettingsElement = doc.Descendants("appSettings").First(); //.First()
            appSettingsElement.Add(xmlAddElement);
        }

        public static ObservableCollection<ConnectionStrings> FindConnectionsStrings(string fileSystemEntry)
        {
            ObservableCollection<ConnectionStrings> ConnectionStringsCollection = new ObservableCollection<ConnectionStrings>();
                var doc = XDocument.Load(fileSystemEntry);
                var elements = doc.Descendants("connectionStrings").Elements();

                foreach (var element in elements)
                {
                    ConnectionStrings connectionStrings = new ConnectionStrings();
                    connectionStrings.Name = (string)element.Attribute("name");
                    connectionStrings.NewConnectionString = (string)element.Attribute("connectionString");
                    connectionStrings.ProviderName = (string)element.Attribute("providerName");
                    connectionStrings.IsValueNew = true;
                    ConnectionStringsCollection.Add(connectionStrings);
                }
            return ConnectionStringsCollection;
        }

        public static  Dictionary<string, ConnectionStrings> CreateConnectionStringsDicitionary(ObservableCollection<ConnectionStrings> connectionStringsCollection)
        {
            Dictionary<string, ConnectionStrings> connectionStringsDictionary = new Dictionary<string, ConnectionStrings>();
            foreach (var connectionString in connectionStringsCollection)
            {
                string name = connectionString.Name;
                connectionStringsDictionary.Add(name, connectionString);
            }
            return connectionStringsDictionary;
        }

        public static ObservableCollection<ConnectionStrings> CompareConnectionStrings(string existingFileSystemEntry, string downloadedFileSystemEntry)
        {
            ObservableCollection<ConnectionStrings> comparedConnectionStringsCollection = new ObservableCollection<ConnectionStrings>();

            HashSet<string> KeySet = new HashSet<string>();
            FindConnectionStringsNames(existingFileSystemEntry, KeySet);
            FindConnectionStringsNames(downloadedFileSystemEntry, KeySet);
            foreach (var name in KeySet)
            {
                ConnectionStrings connectionStrings = new ConnectionStrings();
                connectionStrings.Name = name;
                string downloadedConnectionStringsValue = FindConnectionStringValue(downloadedFileSystemEntry, name);
                if (!string.IsNullOrEmpty(downloadedConnectionStringsValue))
                {
                    connectionStrings.IsValueNew = true;

                }
                string existingConnectionStringValue = FindConnectionStringValue(existingFileSystemEntry, name);
                if (!string.IsNullOrEmpty(existingConnectionStringValue))
                {
                    connectionStrings.IsValueExist = true;
                }
                connectionStrings.NewConnectionString = downloadedConnectionStringsValue;
                connectionStrings.ExistingConnectionString = existingConnectionStringValue;
                comparedConnectionStringsCollection.Add(connectionStrings);
            }

            return comparedConnectionStringsCollection;

        }

        private static void FindConnectionStringsNames(string fileSystemEntry, HashSet<string> KeySet)
        {
            var doc = XDocument.Load(fileSystemEntry);
            var elements = doc.Descendants("connectionStrings").Elements();
            foreach (var item in elements)
            {
                KeySet.Add((string)item.Attribute("name"));
            }
        }

        private static string FindConnectionStringValue(string confFilePath, string name)
        {
            var doc = XDocument.Load(confFilePath);
            var elements = doc.Descendants("connectionStrings").Elements();
            foreach (var item in elements)
            {
                if ((string)item.Attribute("name") == name)
                {
                    return (string)item.Attribute("connectionString");
                }
                else
                {
                }
            }
            return null;
        }

        public static Dictionary<string, ConnectionStrings> CreateComparedConnectionStringsDicitionary(ObservableCollection<ConnectionStrings> comparedConnectionStringsCollection)
        {
            Dictionary<string, ConnectionStrings> newAppSettingsDictionary = new Dictionary<string, ConnectionStrings>();
            foreach (var connectionString in comparedConnectionStringsCollection)
            {
                string name = connectionString.Name;
                newAppSettingsDictionary.Add(name, connectionString);
            }
            return newAppSettingsDictionary;
        }

        public static void WriteConnectionStringstoConFile(string appConfigPath, Dictionary<string, ConnectionStrings> connectionStringsDic)
        {
            try
            {
                var doc = XDocument.Load(appConfigPath);
                var elements = doc.Descendants("connectionStrings").Elements();
                foreach (var connectionString in connectionStringsDic)
                {
                    string key = connectionString.Key;
                    ConnectionStrings conf = connectionString.Value;
                    if (conf.RbExistingValue == true)
                    {
                        foreach (var item in elements)
                        {
                            if (connectionString.Key == (string)item.Attribute("name"))
                            {
                                item.Attribute("connectionString").Value = conf.ExistingConnectionString;
                                break;
                            }
                        }
                    }
                    if (conf.ExistingConnectionString =="")
                    {
                        //AddKeyToConfFile(appsettings, elements, doc, conf);
                        AddNewConnectionStringtoConFile(doc, connectionString, conf);
                    }
                    else
                    {
                        foreach (var item in elements)
                        {
                            if (connectionString.Key == (string)item.Attribute("name"))
                            {
                                item.Attribute("connectionString").Value = conf.NewConnectionString;
                                break;
                            }
                        }
                    }
                }
                doc.Save(appConfigPath);
            }
            catch
            {

            }
        }

        public static void AddNewConnectionStringtoConFile(XDocument doc, KeyValuePair<string, ConnectionStrings> connectionstrings, ConnectionStrings conf)
        {
            XElement xmlAddElement = new XElement("add");
            XAttribute configValueAttribute = new XAttribute("connectionString", conf.NewConnectionString.ToString());
            XAttribute configKeyAttribute = new XAttribute("name", connectionstrings.Key.ToString());
            xmlAddElement.Add(configKeyAttribute);
            xmlAddElement.Add(configValueAttribute);
            XElement appSettingsElement = doc.Descendants("connectionStrings").First(); //.First()
            appSettingsElement.Add(xmlAddElement);
        }

        public static string GetServiceName(string downloadedConfigFilePath)
        {
            string fileName = Path.GetFileName(downloadedConfigFilePath);
            //string serviceName =
            string[] substrings = fileName.Split('.');
            foreach (var substring in substrings)
            {
                if (substring != "exe" || substring != "config")
                {
                    return substring;
                }
            }  
            return "";
        }
    }
}
