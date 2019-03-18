using CommonLibary.Poco;
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
        /// <summary>
        /// Finds appsettings file, ends with ".exe.config" from specified directory and subdirectories.
        /// </summary>
        /// <param name="folderPath">FolderPath, where file is looked for</param>
        /// <returns>Returns filePath that matches or empty string, if file was not found</returns>
        public static string FindAppSettingsFile(string folderPath)
        {
            foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories)) 
            {
                if (!File.Exists(fileSystemEntry)) continue;
                if (fileSystemEntry.EndsWith(".exe.config")) return fileSystemEntry;
            }
            return string.Empty;
        }
        /// <summary>
        /// Finds zipFile from specified directory and subdirectories
        /// </summary>
        /// <param name="folderPath">FolderPath, where file is looked for</param>
        /// <returns>Returns zipFilePath or empty string, if file was not found</returns>
        public static string FindZipFile(string folderPath)
        {
            foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories)) 
            {
                if (!File.Exists(fileSystemEntry)) continue; 
                if (fileSystemEntry.EndsWith(".zip")) return fileSystemEntry;
            }
            return string.Empty;
        }
        /// <summary>
        /// Finds appsettings from configuration file
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <returns>Returns appSettings collections.</returns>
        public static ObservableCollection<AppSettingsConfig> FindAppSettings(string filePath)
        {
            ObservableCollection<AppSettingsConfig> appSettingsCollection = new ObservableCollection<AppSettingsConfig>();

                var doc = XDocument.Load(filePath);
                var elements = doc.Descendants("appSettings").Elements();

                foreach (var item in elements)
                {
                    AppSettingsConfig appSetting = new AppSettingsConfig();
                    appSetting.Key = (string)item.Attribute("key");
                    appSetting.NewValue = (string)item.Attribute("value");
                    appSetting.IsValueNew = true;
                    appSettingsCollection.Add(appSetting);
                }
            return appSettingsCollection;
        }
        /// <summary>
        /// Compares existing and dowloaded appSettings in configuration file.
        /// </summary>
        /// <param name="existingFilePath">Existing configuration file path</param>
        /// <param name="downloadedFilePath">Downloaded configuration file path</param>
        /// <returns>Returns new appSettings collection</returns>
        public static ObservableCollection<AppSettingsConfig> CompareAppSettings( string existingFilePath, string downloadedFilePath)
        {
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = new ObservableCollection<AppSettingsConfig>();

                HashSet<string> KeySet = new HashSet<string>();
                string element = "appSettings";
                string attributeKey = "key";
                string attributeValue = "value";
                FindXMlElementAttributes(existingFilePath, KeySet, element, attributeKey);
                FindXMlElementAttributes(downloadedFilePath, KeySet, element, attributeKey);
                foreach (var key in KeySet)
                {
                    AppSettingsConfig appSetting = new AppSettingsConfig();
                    appSetting.Key = key;
                    string downloadedAppSettingValue = FindAttributeValue(downloadedFilePath, key, element, attributeKey, attributeValue);
                    if (!string.IsNullOrEmpty(downloadedAppSettingValue))
                    {
                        appSetting.IsValueNew = true;
                        
                    }
                    string existingAppSettingValue = FindAttributeValue(existingFilePath, key, element, attributeKey, attributeValue);
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
        /// <summary>
        /// Creates appSettings dictionary from appSettings collection.
        /// </summary>
        /// <param name="appSettingsCollection">AppSettings collection</param>
        /// <returns>Returns appSettings dictionary.</returns>
        public static Dictionary<string, AppSettingsConfig> CreateAppSettingsDicitionary(ObservableCollection<AppSettingsConfig> appSettingsCollection)
        {
            Dictionary<string, AppSettingsConfig> newAppSettingsDictionary = new Dictionary<string, AppSettingsConfig>();
            foreach (var appSetting in appSettingsCollection)
            {
                string key = appSetting.Key;
                try
                {
                newAppSettingsDictionary.Add(key, appSetting);
                }
                catch { throw;}
            }
            return newAppSettingsDictionary;
        }
        /// <summary>
        /// Finds XML element attributes  from configuration file and add them to HashSet.
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <param name="KeySet">Attributes collection</param>
        private static void FindXMlElementAttributes(string filePath, HashSet<string> KeySet, string XMLElement, string XMLAttribute)
        {
            var doc = XDocument.Load(filePath);
            var elements = doc.Descendants(XMLElement).Elements();
            foreach (var item in elements)
            {
                KeySet.Add((string)item.Attribute(XMLAttribute));
            }
        }
        /// <summary>
        /// Writes appsettings to configuration file. If attribute exists adds new value. If attribute does not exist, adds new element.
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <param name="appSettingsDic">AppSettings dictionary</param>
        public static void WriteSettingsToConfFile(string filePath, Dictionary<string, AppSettingsConfig> appSettingsDic)
        {
            var doc = XDocument.Load(filePath);
            var elements = doc.Descendants("appSettings").Elements();
            foreach (var appsettings in appSettingsDic)
            {
                string key = appsettings.Key;
                AppSettingsConfig conf = appsettings.Value;
                // If configuration file consists specified appSetting key, replaces value element value, matching this key.
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
                // If configuration file do not consists specified appSetting key, add new key and value pair to appSettings.
                else 
                {
                    if (conf.ExistingValue == null)
                    {
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
            }
            doc.Save(filePath);
        }
        /// <summary>
        /// Adds new appSettings element add to configuration file.
        /// </summary>
        /// <param name="doc">XML document</param>
        /// <param name="appsettings">Configuration file appsettings key-value pairs</param>
        /// <param name="conf">AppSettings object</param>
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
        /// <summary>
        /// Finds connectionStrings from configuration file.
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <returns>Returns connectionStrings collections.</returns>
        public static ObservableCollection<ConnectionStrings> FindConnectionsStrings(string filePath)
        {
            ObservableCollection<ConnectionStrings> ConnectionStringsCollection = new ObservableCollection<ConnectionStrings>();
                var doc = XDocument.Load(filePath);
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
        /// <summary>
        /// Creates connectionStrings dictionary from connectionStrings collection.
        /// </summary>
        /// <param name="connectionStringsCollection">ConnectionStrings collection</param>
        /// <returns>Returns connectionStrings dictionary.</returns>
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
        /// <summary>
        /// Compares existing and dowloaded connectionStrings in configuration file.
        /// </summary>
        /// <param name="existingFilePath">Existing configuration file path</param>
        /// <param name="downloadedFilePath">Downloaded configuration file path</param>
        /// <returns>Returns new connectionStrings collection</returns>
        public static ObservableCollection<ConnectionStrings> CompareConnectionStrings(string existingFilePath, string downloadedFilePath)
        {
            ObservableCollection<ConnectionStrings> comparedConnectionStringsCollection = new ObservableCollection<ConnectionStrings>();

            HashSet<string> KeySet = new HashSet<string>();
            string XMLElement = "connectionStrings";
            string attributeName = "name";
            string attributeConnectionString = "connectionString";
            FindXMlElementAttributes(existingFilePath, KeySet, XMLElement, attributeName);
            FindXMlElementAttributes(downloadedFilePath, KeySet, XMLElement, attributeName);
            foreach (var name in KeySet)
            {
                ConnectionStrings connectionStrings = new ConnectionStrings();
                connectionStrings.Name = name;
                string downloadedConnectionStringsValue = FindAttributeValue(downloadedFilePath, name, XMLElement, attributeName, attributeConnectionString);
                if (!string.IsNullOrEmpty(downloadedConnectionStringsValue))
                {
                    connectionStrings.IsValueNew = true;
                }
                string existingConnectionStringValue = FindAttributeValue(existingFilePath, name, XMLElement, attributeName, attributeConnectionString);
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
        /// <summary>
        /// Finds attribute value that matches with specified attribute value from configuration file.
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <param name="name">Hachset collection element</param>
        /// <param name="XMLElement">XML element name</param>
        /// <param name="attribute1">The name of the attribute we are looking for</param>
        /// <param name="attribute2">Attribute what we looking for</param>
        /// <returns>Return attribute or null if does not find specified attribute</returns>
        private static string FindAttributeValue(string filePath, string name, string XMLElement, string attribute1, string attribute2)
        {
            var doc = XDocument.Load(filePath);
            var elements = doc.Descendants(XMLElement).Elements();
            foreach (var item in elements)
            {
                if ((string)item.Attribute(attribute1) == name)
                {
                    return (string)item.Attribute(attribute2);
                }
            }
            return null;
        }
        /// <summary>
        /// Creates connectionStrings dictionary from connectionStrings collection.
        /// </summary>
        /// <param name="comparedConnectionStringsCollection">ConnectionStrings collection</param>
        /// <returns>Returns connectionStrings dictionary.</returns>
        public static Dictionary<string, ConnectionStrings> CreateComparedConnectionStringsDicitionary(ObservableCollection<ConnectionStrings> comparedConnectionStringsCollection)
        {
            Dictionary<string, ConnectionStrings> newAppSettingsDictionary = new Dictionary<string, ConnectionStrings>();
            foreach (var connectionString in comparedConnectionStringsCollection)
            {
                string name = connectionString.Name;
                try
                {
                    newAppSettingsDictionary.Add(name, connectionString);
                }
                catch { throw; }
            }
            return newAppSettingsDictionary;
        }
        /// <summary>
        /// Writes connectionStrings elements to configuration file. If attribute exists adds new value. If attribute does not exist, adds new element.
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <param name="connectionStringsDic">ConnectionStrings dictionary</param>
        public static void WriteConnectionStringstoConFile(string filePath, Dictionary<string, ConnectionStrings> connectionStringsDic)
        {
            try
            {
                var doc = XDocument.Load(filePath);
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
                doc.Save(filePath);
            }
            catch{throw; }
        }
        /// <summary>
        /// Adds new connectionStrings elements to configuration file.
        /// </summary>
        /// <param name="doc">XML document</param>
        /// <param name="connectionstrings">Configuration file connectionString attributes</param>
        /// <param name="conf">ConnectionString object</param>
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
        /// <summary>
        /// Gets service name from configuration file path
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <returns>Returns service name</returns>
        public static string GetServiceName(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string[] substrings = fileName.Split('.');
            List<string> serviceNames = new List<string>();
            foreach (string substring in substrings)
            {
                if (substring != "exe" && substring != "config")
                {
                    serviceNames.Add(substring);
                }
            }
            string serviceName = string.Join(".", serviceNames.ToArray());         
            return serviceName;
        }
        /// <summary>
        /// Gets service exe file name from configuration file path
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <returns>Returns service exe file path</returns>
        public static string GetExeFileName(string filePath)
        {
            string exeFilePath ="";
            if (filePath.EndsWith(".config"))
            {
                exeFilePath = filePath.Substring(0, filePath.LastIndexOf(".config"));
                return exeFilePath;
            }
            return exeFilePath;
        }
    }
}
