using MicroServiceInstaller3.Poco;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace MicroServiceInstaller3
{
    class ConfFileHandler
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

        public static ObservableCollection<AppSettingsConfig> FindConfSettings(string fileSystemEntry)
        {
            ObservableCollection<AppSettingsConfig> appSettingsCollection = new ObservableCollection<AppSettingsConfig>();

                var doc = XDocument.Load(fileSystemEntry);
                var elements = doc.Descendants("appSettings").Elements();

                foreach (var item in elements)
                {
                    AppSettingsConfig appSetting = new AppSettingsConfig();
                    appSetting.Key = (string)item.Attribute("key");
                    appSetting.NewValue = (string)item.Attribute("value"); // muutsin newvalue-> value
                    appSettingsCollection.Add(appSetting);
                }
            return appSettingsCollection;
        }
        
        public static ObservableCollection<AppSettingsConfig> CompareAppSettings( string existingFileSystemEntry, string downloadedFileSystemEntry)
        {
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = new ObservableCollection<AppSettingsConfig>();

                HashSet<string> KeySet = new HashSet<string>();
                FindKeys(existingFileSystemEntry, KeySet);
                FindKeys(downloadedFileSystemEntry, KeySet);
                foreach (var key in KeySet)
                {
                    AppSettingsConfig appSetting = new AppSettingsConfig();
                    appSetting.Key = key;
                    string downloadedAppSettingValue = FindValue(downloadedFileSystemEntry, key);
                    string existingAppSettingValue = FindValue(existingFileSystemEntry, key);
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

        private static void FindKeys(string fileSystemEntry, HashSet<string> KeySet)
        {
            var doc = XDocument.Load(fileSystemEntry);
            var elements = doc.Descendants("appSettings").Elements();
            foreach (var item in elements)
            {
                KeySet.Add((string)item.Attribute("key"));
            }
        }

        private static string FindValue(string confFilePath, string key)
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

        public static Dictionary<string, AppSettingsConfig> ReadModifiedConfSettings( ObservableCollection<AppSettingsConfig> modifiedAppSettings)
        {
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = new Dictionary<string, AppSettingsConfig>();
            foreach (var item in modifiedAppSettings)
            {
                AppSettingsConfig appSetting = item as AppSettingsConfig;
                appSettingsDictionary.Add(appSetting.Key, appSetting);
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
                if (conf.RbExistingValueVisibility == Visibility.Hidden)
                {
                    //AddKeyToConfFile(appsettings, elements, doc, conf);
                    AddSettingtoConFile(doc, appsettings, conf);
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
            doc.Save(appConfigPath);
        }

        private static void AddSettingtoConFile(XDocument doc, KeyValuePair<string, AppSettingsConfig> appsettings, AppSettingsConfig conf)
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
                    connectionStrings.ConnectionString = (string)element.Attribute("connectionString");
                    connectionStrings.ProviderName = (string)element.Attribute("providerName");
                    ConnectionStringsCollection.Add(connectionStrings);
                }
            return ConnectionStringsCollection;
        }
    }
}
