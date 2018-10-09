using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Transactions;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.ComponentModel;
using System.Collections;

namespace MicroServiceInstaller3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FShandler.MakeFolders(zipFileLabel: LbZipFilesFolder, workFilesLabel: LbworkFilesFolder, finalZipLabel: LbFinalZipFolder);
        }
        string RandomFileName = "";

        private void BSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select directory";
            folderBrowserDialog1.ShowNewFolderButton = false;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = FShandler.ChooseFolder(folderBrowserDialog1, selectedFolderLabel: LbSelectedFolder, savebutton: BnZip);
                string workFilesFolderPath = LbworkFilesFolder.Content.ToString();
                FShandler.DirectoryCopy(selectedPath, workFilesFolderPath, copySubDirs: true);
                IEnumerable<string> unFilteredFileList = CreateUnFilteredFileList(workFilesFolderPath);
                FilterFileList(unFilteredFileList);
                FShandler.CreateMetaDataFile(selectedPath, workFilesFolderPath);
            }
        }

        private void FilterFileList(IEnumerable<string> unFilteredFileList)
        {
            foreach (var value in unFilteredFileList) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
            {
                if (!File.Exists(value)) continue; // kui fail eksisteerib, j'tkab

                bool endsIn = (value.EndsWith(".pdb") || value.Contains("/deps/") || value.Contains(".vshost.")); // kui faili asukohanimetus sisaldab j'rgmis v''rtusi
                if (endsIn)
                {
                    File.Delete(value); // kustutab faili
                }
                if (!endsIn)
                {
                    ListFiles.Items.Add($"{value}");
                }
            }
        }

        private IEnumerable<string> CreateUnFilteredFileList(string temporaryFolder)
        {
            IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(temporaryFolder, "*", SearchOption.AllDirectories); // Otsib ajutisest kaustast ja alamkaustadest faile
            ListFiles.ItemsSource = Files; // Paigutab failid faililisti
            IEnumerable<string> unFilteredFileList = (IEnumerable<string>)ListFiles.ItemsSource; //muudab valitud faili asukohanimetuse tekstiks
            ListFiles.ItemsSource = null; // m''rab, et alguses on faililist t[hi
            return unFilteredFileList;
        }

        private void ListFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListFiles.SelectedIndex >= 0)
            {
                BnConfig.IsEnabled = true;
                LbTemporary.Content = ListFiles.SelectedItem.ToString() ;
            }
            else
            {
                LbProcessStatus.Content = "You have to select file first";
            }
        }

        private void BnConfig_Click(object sender, RoutedEventArgs e)
        {
            string confFilePath = LbTemporary.Content.ToString();
            LvUploadedConfigSettings.ItemsSource = FindConfSettings(confFilePath, statusLabel: LbProcessStatus);
            LvUploadedConnectionSettings.ItemsSource = FindConnectionsStrings(confFilePath, statusLabel: LbProcessStatus);
            BnSaveChanges.IsEnabled = true;
        }

        private string FindAppSettingsFile(System.Windows.Controls.Label temporaryfolderLabel)
        {

            //string temporaryFolder = handlePropertyChanged(sender, e);
            string temporaryFolder = temporaryfolderLabel.Content.ToString();

            return FindAppSettingsFile(temporaryFolder);
        }

        private string FindAppSettingsFile(string temporaryFolder)
        {
            foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(temporaryFolder, "*", SearchOption.AllDirectories)) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
            {
                if (!File.Exists(fileSystemEntry)) continue; // kui fail ei eksisteeri, j'tkab

                if (fileSystemEntry.EndsWith(".exe.config")) return fileSystemEntry;


            }
            return string.Empty;
        }
        private ObservableCollection<AppSettingsConfig> CompareAppSettings(string existingFileSystemEntry, string downloadedFileSystemEntry, System.Windows.Controls.Label statusLabel)
        {
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = new ObservableCollection<AppSettingsConfig>();
            try
            {
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
                LvDownloadedConfigSettings.ItemsSource = comparedAppSettingsCollection;
            }
            catch //(Exception error)
            {
                statusLabel.Content = "This file do not consist keys";
            }
            return comparedAppSettingsCollection;
        }

        private void FindKeys(string fileSystemEntry, HashSet<string> KeySet)
        {
            var doc = XDocument.Load(fileSystemEntry);
            var elements = doc.Descendants("appSettings").Elements();
            foreach (var item in elements)
            {
                KeySet.Add((string)item.Attribute("key"));               
            }
        }

        private string FindValue(string confFilePath, string key)
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

        private ObservableCollection<AppSettingsConfig> FindConfSettings(string fileSystemEntry, System.Windows.Controls.Label statusLabel)
        {
            ObservableCollection<AppSettingsConfig> appSettingsCollection = new ObservableCollection<AppSettingsConfig>();
            try
            {
                var doc = XDocument.Load(fileSystemEntry);
                var elements = doc.Descendants("appSettings").Elements();

                    foreach (var item in elements)
                    {
                        AppSettingsConfig appSetting = new AppSettingsConfig();
                        appSetting.Key = (string)item.Attribute("key");
                        appSetting.NewValue = (string)item.Attribute("value"); // muutsin newvalue-> value
                        appSettingsCollection.Add(appSetting);
                    }
            }
            catch //(Exception error)
            {
                //statusLabel.Content = error.Message;
                statusLabel.Content = "This file does not consist appsettings, please select another file";
            }
            return appSettingsCollection;
        }

        public class AppSettingsConfig
        {
            public string Key { get; set; }
            public string NewValue { get; set; }
            public string ExistingValue { get; set; }
            public bool RbNewValue { get; set; }
            public bool RbExistingValue { get; set; }
            public Visibility TbValueVisibility { get; set; }
            public Visibility TbExistingValueVisibility { get; set; }
            public Visibility RbNewValueVisibility { get; set; }
            public Visibility RbExistingValueVisibility { get; set; }
            public Thickness TbNewValueBorder { get; set; }
            public Thickness TbExistigValueBorder { get; set; }
        }

        private void BnSaveChanges_Click(object sender, RoutedEventArgs e)
        {        
            string appConfigPath = LbworkFilesFolder.Content.ToString();
            string selectedPath = LbTemporary.Content.ToString();
            Dictionary<string, AppSettingsConfig> appSettingsDictionary;
            ReadModifiedConfSettings(out appSettingsDictionary, out appConfigPath, configSettings: LvUploadedConfigSettings, appSettingsPath: LbappSettingsPath);
            WriteSettingsToConfFile(selectedPath, appSettingsDic: appSettingsDictionary, statusLabel: LbProcessStatus);
            Dictionary<string, ConnectionStrings> connectionStringsDicitionary;
            //ReadModifiedConnectionSettings
            ReadModifiedConnectionStrings(out connectionStringsDicitionary, out appConfigPath, connectionStrings: LvUploadedConnectionSettings, appSettingsPath: LbappSettingsPath);
            //WriteConnectionSettings to confFile
        }

        private static void ReadModifiedConfSettings(out Dictionary<string, AppSettingsConfig> appSettingsDictionary, out string appConfigPath, System.Windows.Controls.ListView configSettings, System.Windows.Controls.Label appSettingsPath)
        {
            var ModifiedAppSettings = configSettings.ItemsSource;
            appSettingsDictionary = new Dictionary<string, AppSettingsConfig>();
            foreach (var item in ModifiedAppSettings)
            {
                AppSettingsConfig appSetting = item as AppSettingsConfig;
                appSettingsDictionary.Add(appSetting.Key, appSetting);
            }
            appConfigPath = appSettingsPath.Content.ToString();
        }

        private static void ReadModifiedConnectionStrings(out Dictionary<string, ConnectionStrings> connectionStringsDicitionary, out string appConfigPath, System.Windows.Controls.ListView connectionStrings, System.Windows.Controls.Label appSettingsPath)
        {
            IEnumerable modifiedConnectionStrings = connectionStrings.ItemsSource;
            connectionStringsDicitionary = new Dictionary<string, ConnectionStrings>();
            foreach (var item in modifiedConnectionStrings)
            {
                ConnectionStrings connectionString = item as ConnectionStrings;
                connectionStringsDicitionary.Add(connectionString.Name, connectionString);
            }
            appConfigPath = appSettingsPath.Content.ToString();
        }

        private void WriteSettingsToConfFile(string appConfigPath, Dictionary<string, AppSettingsConfig> appSettingsDic, System.Windows.Controls.Label statusLabel)
        {
            try
            {
                var doc = XDocument.Load(appConfigPath);
                var elements = doc.Descendants("appSettings").Elements();
                foreach (var appsettings in appSettingsDic)
                {
                    string key = appsettings.Key;
                    AppSettingsConfig conf = appsettings.Value;
                    if(conf.RbExistingValue == true)
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
                    else
                    {
                        SaveValue(elements, appsettings, conf, doc);
                    }
                }
                doc.Save(appConfigPath);
                
                statusLabel.Content = "Changes saved ";
            }
            catch (Exception error)
            {
                statusLabel.Content = error.Message;
            }
        }

        private static void SaveValue(IEnumerable<XElement> elements, KeyValuePair<string, AppSettingsConfig> appsettings, AppSettingsConfig conf, XDocument doc)
        {       
            if (conf.RbExistingValueVisibility == Visibility.Hidden)
            {
                AddKeyToConfFile(appsettings, elements, doc, conf);
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

        private static void AddKeyToConfFile(KeyValuePair<string, AppSettingsConfig> appsettings, IEnumerable<XElement> elements, XDocument doc, AppSettingsConfig conf)
        {
            XElement xmlAddElement = new XElement("add");
            XAttribute configValueAttribute = new XAttribute("value", conf.NewValue.ToString());
            XAttribute configKeyAttribute = new XAttribute("key", appsettings.Key.ToString());
            xmlAddElement.Add(configKeyAttribute);
            xmlAddElement.Add(configValueAttribute);
            XElement appSettingsElement = doc.Descendants("appSettings").First(); //.First()
            appSettingsElement.Add(xmlAddElement);
        }

        private void BnZip_Click(object sender, RoutedEventArgs e)
        {
            string InitialsFilesFolder = LbworkFilesFolder.Content.ToString();
            string zipFileFolder = LbZipFilesFolder.Content.ToString();
            string zipFile = System.IO.Path.Combine(zipFileFolder, RandomFileName + ".zip"); // M''rab zip faili asukoha ja nime

            BnConfig.IsEnabled = false;
            BnSaveChanges.IsEnabled = false;
            using (var scope = new TransactionScope())
            {
               // File.Delete(zipFile); // kustutab faili, mis asub sellel aadressil

                ZipFile.CreateFromDirectory(InitialsFilesFolder, zipFile); // loob zip faili

                bool existItem = false;
                foreach (var value in ListZipFiles.Items) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
                {
                    string ZipFilePath = value.ToString();
                    existItem = (ZipFilePath.Equals(zipFile)); // kui on olemas sama asukohanimetusega fail
                    if (existItem)
                    {
                        break;
                    }
                }

                if (!existItem)
                {
                    ListZipFiles.Items.Add($"{zipFile}");
                }

                ListFiles.Items.Clear(); // eemaldab listis olevad asukohakirjed
                LbSelectedFolder.Content = ""; // eemaldab valitud algse kataloogi asukoha kirje.
                LbappSettingsPath.Content = "";
                LvUploadedConfigSettings.ItemsSource = "";
                LbProcessStatus.Content = "";

                if (ListZipFiles.HasItems)
                {
                    BnFinishandZip.IsEnabled = true;
                }
                scope.Complete();
            }
        }

        private void BnFinishandZip_Click(object sender, RoutedEventArgs e)
        {
            BnZip.IsEnabled = false;
            using (var scope = new TransactionScope())
            {
               string zipLocation = LbZipFilesFolder.Content.ToString();

                string finalZipFileName = System.IO.Path.Combine(LbFinalZipFolder.Content.ToString(), "final.zip");
                File.Delete(finalZipFileName);

                ZipFile.CreateFromDirectory(zipLocation, finalZipFileName);
                string finalLocation = System.IO.Path.Combine(zipLocation, finalZipFileName);

                LbStatus.Content = "Zip file is created successfully: " + finalLocation;

                ListZipFiles.Items.Clear(); // eemaldab listis olevad valitud zip failide asukohakirjed
                scope.Complete();
            }
        }

        private void BSelectZipFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog zipFileBrowserDialog1 = new OpenFileDialog();

            zipFileBrowserDialog1.DefaultExt = ".zip";

            DialogResult result = zipFileBrowserDialog1.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string zipFileName = zipFileBrowserDialog1.FileName;
                LbSelectedZipFile.Content = zipFileName;
                string extractDirectory = FShandler.CreateExtractFolder1();
                ZipFile.ExtractToDirectory(zipFileName, extractDirectory);
                IEnumerable<string> unFilteredZipFileList = CreateUnFilteredZipFileList(extractDirectory);
                foreach (var zipFile in unFilteredZipFileList)
                {
                    bool endsIn = (zipFile.EndsWith(".zip"));
                    if (endsIn)
                    {
                        string extractDirectory2 = FShandler.CreateExtractFolder1();
                        ZipFile.ExtractToDirectory(zipFile, extractDirectory2);
                        IEnumerable<string> unFilteredFileList = CreateUnFilteredZipFileList(extractDirectory2);
                        FilterZipFileList(unFilteredFileList);
                    }              
                }
                FilterZipFileList(unFilteredZipFileList);
            }
        }

        private void FilterZipFileList(IEnumerable<string> unFilteredFileList)
        {
            foreach (var value in unFilteredFileList) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
            {
                if (!File.Exists(value)) continue; // kui fail eksisteerib, j'tkab

                bool endsIn = (value.EndsWith(".exe.config")); // kui faili asukohanimetus sisaldab j'rgmis v''rtusi
                if (endsIn)
                {
                    ListAppSettingsFiles.Items.Add($"{value}");
                    //ListFiles.Items.Add($"{value}");                  
                }
            }
        }

        private IEnumerable<string> CreateUnFilteredZipFileList(string extractPath)
        {
            IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(extractPath, "*", SearchOption.AllDirectories); // Otsib ajutisest kaustast ja alamkaustadest faile
            ListUnPackedZipFiles.ItemsSource = Files; // Paigutab failid faililisti
            IEnumerable<string> unFilteredZipFileList = (IEnumerable<string>)ListUnPackedZipFiles.ItemsSource; //muudab valitud faili asukohanimetuse tekstiks
            return unFilteredZipFileList;
        }

        public class ConfFileInfo
        {
            private string _selectedFile;

            //public string Path { get; set; }
            public string selectedFile
            {
                get { return _selectedFile; }
                set { _selectedFile = value; }
            }

            public static implicit operator string(ConfFileInfo v)
            {
                throw new NotImplementedException();
            }
        }

        private void BnConfigDownloadedAppSettings_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select directory";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = FShandler.ChooseFolder(folderBrowserDialog1, selectedFolderLabel: LbExistingAppSettingsFilePath , savebutton: BnSaveDownloadedAppSettingsChanges );
                LbTemporaryFolderZipFile.Content = selectedPath;
                LbExistingAppSettingsFilePath.Content = selectedPath; // n'itab valitud kausta Seda rida pole vaja, toimub chooseFolderi funktsioonis!!!!
                string temporaryFolder = LbTemporary.Content.ToString();
                string[] folderIsEmpty = Directory.GetFiles(selectedPath);
                if (folderIsEmpty.Length == 0)
                {
                    DirectoryInfo diSource = new DirectoryInfo(temporaryFolder);
                    DirectoryInfo diTarget = new DirectoryInfo(selectedPath);
                    FShandler.CopyAll(diSource, diTarget);
                    string confFilePath = FindAppSettingsFile(selectedPath);
                    ObservableCollection<AppSettingsConfig> appSettingsCollection = FindConfSettings(confFilePath, statusLabel: LbDownloadedProcessStatus);
                    LvDownloadedConfigSettings.ItemsSource = appSettingsCollection;
                    AddRadioButtons(appSettingsCollection);
                    HideEmptyTextBox(appSettingsCollection);
                    BnSaveDownloadedAppSettingsChanges.IsEnabled = true;
                }
                else
                {
                    string existingConfFilePath = FindAppSettingsFile(selectedPath);
                    string downloadedConfFilePath = FindAppSettingsFile(temporaryFolder);
                    string existingConfFileName = System.IO.Path.GetFileName(path: existingConfFilePath);
                    string downloadedConfFileName = System.IO.Path.GetFileName(downloadedConfFilePath);
                    if (existingConfFileName == downloadedConfFileName)
                    {
                        LbDownloadedAppSettingsFilePath.Content = downloadedConfFilePath;
                        ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = CompareAppSettings(existingConfFilePath , downloadedConfFilePath, statusLabel: LbDownloadedProcessStatus);
                        AddRadioButtons(comparedAppSettingsCollection);
                        HideEmptyTextBox(comparedAppSettingsCollection);
                       // AddThickBorder(comparedAppSettingsCollection);
                        BnSaveDownloadedAppSettingsChanges.IsEnabled = true;
                    }
                    else
                    {
                        LbExistingAppSettingsFilePath.Content = "Existing conf file name does not match with downloaded conf file name. Please select another folder";
                        LbDownloadedAppSettingsFilePath.Content = "Existing conf file name does not match with downloaded conf file name. Please select another folder";
                    }
                }
            }
        }

        private void AddThickBorder(ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection)
        {
            foreach (var item in comparedAppSettingsCollection)
            {
                if (item.RbExistingValue == true)
                {
                    item.TbExistigValueBorder = new Thickness(3);
                }
                if (item.RbNewValue == true)
                {
                    item.TbNewValueBorder = new Thickness(3);
                }
            }
        }

        private void HideEmptyTextBox(ObservableCollection<AppSettingsConfig> appSettings)
        {
            foreach (var item in appSettings)
            {
                if (string.IsNullOrEmpty(item.ExistingValue))
                {
                    item.TbExistingValueVisibility = Visibility.Hidden;
                    item.RbExistingValueVisibility = Visibility.Hidden;
                }
                if (string.IsNullOrEmpty(item.NewValue))
                {
                    item.TbValueVisibility = Visibility.Hidden;
                    item.RbNewValueVisibility = Visibility.Hidden;
                }
            }
        }

        private void AddRadioButtons(ObservableCollection<AppSettingsConfig> appSettings)
        {
            foreach (var item in appSettings)
            {
                if (!string.IsNullOrEmpty(item.ExistingValue))
                {
                    item.RbExistingValue = true;
                }
                else
                {
                    item.RbNewValue = true;
                }
            }

        }

        private void BnSaveDownloadedAppSettingsChanges_Click(object sender, RoutedEventArgs e)
        {
            string downloadedConfigFilePath = LbDownloadedAppSettingsFilePath.Content.ToString();
            string existingConfigFileDirectory = LbExistingAppSettingsFilePath.Content.ToString();
            string existingConfFilePath = System.IO.Path.Combine(existingConfigFileDirectory, System.IO.Path.GetFileName(downloadedConfigFilePath));
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = CreateComparedAppSettingsDicitionary(appsettingslist: LvDownloadedConfigSettings);            
            WriteSettingsToConfFile(existingConfFilePath, appSettingsDic: appSettingsDictionary, statusLabel: LbDownloadedProcessStatus);
        }

        private Dictionary<string, AppSettingsConfig> CreateComparedAppSettingsDicitionary(System.Windows.Controls.ListView appsettingslist)
        {
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = appsettingslist.ItemsSource as ObservableCollection<AppSettingsConfig>;
            Dictionary<string, AppSettingsConfig> newAppSettingsDictionary = new Dictionary<string, AppSettingsConfig>();
            foreach (var appSetting in comparedAppSettingsCollection)
            {
                string key = appSetting.Key;
                newAppSettingsDictionary.Add(key, appSetting);           
            }
            return newAppSettingsDictionary;
        }

        private void ListAppSettingsFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LbDownloadedAppSettingsFilePath.Content = "";
            LbDownloadedProcessStatus.Content = "";
            if (ListAppSettingsFiles.SelectedIndex >= 0)
            {
                BnConfigDownloadedAppSettings.IsEnabled = true;
                LbTemporary.Content = ListAppSettingsFiles.SelectedItem;
                string selectedFile = LbTemporary.Content.ToString();
                string filePath = System.IO.Path.GetDirectoryName(selectedFile);
                LbTemporary.Content = filePath;
                LbDownloadedAppSettingsFilePath.Content = filePath;
            }
            else
            {
                LbDownloadedProcessStatus.Content = "You have to select config file first";
            }
        }

        private void Tab_Drop(object sender, System.Windows.DragEventArgs e)
        {

        }

        private ObservableCollection<ConnectionStrings> FindConnectionsStrings(string fileSystemEntry, System.Windows.Controls.Label statusLabel)
        {
            ObservableCollection<ConnectionStrings> ConnectionStringsCollection = new ObservableCollection<ConnectionStrings>();
            try
            {
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
            }
            catch //(Exception error)
            {
                //statusLabel.Content = error.Message;
                statusLabel.Content = "This file does not consist connectionSettings, please select another file";
            }
            return ConnectionStringsCollection;
        }

        public class ConnectionStrings
        {
            public string Name { get; set; }
            public string ConnectionString { get; set; }
            public string ProviderName { get; set; }
        }
    }
}
