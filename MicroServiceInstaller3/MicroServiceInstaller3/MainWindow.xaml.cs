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
            MakeFolders();
        }

        private  void MakeFolders()
        {
            string temporaryFolderPath = System.IO.Path.GetTempPath();

            string zipPath = System.IO.Path.Combine(temporaryFolderPath, "ZipDirectory");
            FShandler.CreateDirectory(zipPath);
            LbZipFilesFolder.Content = zipPath;

            string workDirectoryPath = System.IO.Path.Combine(temporaryFolderPath, "tempDirectory");
            LbworkFilesFolder.Content = workDirectoryPath;
            FShandler.CreateDirectory(workDirectoryPath);

           
            string finalZipLocation = System.IO.Path.Combine("C:\\", "FinalZip");
            LbFinalZipFolder.Content = finalZipLocation;
            FShandler.CreateDirectory(finalZipLocation);

            string temporaryConfFileLocation = System.IO.Path.Combine(temporaryFolderPath, "tempConfFile");
            LbTemporaryComparedConfFilePath.Content = temporaryConfFileLocation;
            FShandler.CreateDirectory(temporaryConfFileLocation);
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
                string selectedPath = ChooseFolder(folderBrowserDialog1);

                string workFilesFolderPath = LbworkFilesFolder.Content.ToString();

                FShandler.DirectoryCopy(selectedPath, workFilesFolderPath, copySubDirs: true);
                IEnumerable<string> unFilteredFileList = CreateUnFilteredFileList(workFilesFolderPath);
                FilterFileList(unFilteredFileList);
                CreateMetaDataFile(selectedPath, workFilesFolderPath);
            }
        }

        private void CreateMetaDataFile(string SelectedPath, string temporaryFolder)
        {
            RandomFileName = Guid.NewGuid().ToString();

            string path = System.IO.Path.Combine(temporaryFolder, RandomFileName + ".txt");
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(SelectedPath);
                }
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

        private string ChooseFolder(FolderBrowserDialog folderBrowserDialog1)
        {
            string selectedPath = folderBrowserDialog1.SelectedPath; // Loob muutuja, mis vastab valitud kaustale
            LbSelectedFolder.Content = selectedPath;// M''rab, kuhu kuvatakse valitud kausta sisu
            if (LbSelectedFolder.HasContent)
            {
                BnConfig.IsEnabled = true;
                BnZip.IsEnabled = true;
            }
            return selectedPath;
        }

        private void BnConfig_Click(object sender, RoutedEventArgs e)
        {
            string confFilePath = FindAppSettingsFile(LbworkFilesFolder);
            LvUploadedConfigSettings.ItemsSource = FindConfSettings(confFilePath, statusLabel: LbProcessStatus);
            //ListAppSettings(confFilePath, appSettingsPath: LbappSettingsPath, configSettings: LvUploadedConfigSettings, saveChanges: BnSaveChanges);

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
        private ObservableCollection<AppSettingsConfig> CompareAppSettings(string existingFileSystemEntry, string downloadedFileSystemEntry)
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
            catch (Exception error)
            {
                //statusLabel.Content = error.Message;
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
                        appSetting.NewValue = (string)item.Attribute("newvalue");
                        appSettingsCollection.Add(appSetting);
                    }
            }
            catch (Exception error)
            {
                statusLabel.Content = error.Message;
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
            Dictionary<string, AppSettingsConfig> appSettingsDictionary;
            string appConfigPath;
            ReadModifiedConfSettings(out appSettingsDictionary, out appConfigPath, configSettings: LvUploadedConfigSettings, appSettingsPath:LbappSettingsPath);
            WriteSettingsToConfFile(appConfigPath, appSettingsDic: appSettingsDictionary, statusLabel: LbProcessStatus);
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
                statusLabel.Content = "Changes saved";
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

            //xmlAddElement = doc.  CreateElement("add");
            //xmlAddElement.Attributes.Append("key", appsettings.Value.ToString());
            //xmlAddElement.Attributes.Append("value", appsettings.Key.ToString());
            //doc.Descendants("appSettings").AppendChild(xmlAddElement);
        }



        //XElement element = new XElement("Conn");
        //XAttribute attribute = new XAttribute("Server", comboBox1.Text);
        //element.Add(attribute);

        //2
        //down vote
        //XmlDocument doc = new XmlDocument();
        //        doc.Load("input.xml");

        //XmlElement records = doc.CreateElement("Records");
        //        records.InnerText = Guid.NewGuid().ToString();
        //        doc.DocumentElement.AppendChild(records);

        //doc.Save("output.xml"); 

        private void BnZip_Click(object sender, RoutedEventArgs e)
        {
            string InitialsFilesFolder = LbworkFilesFolder.Content.ToString();
            string zipFileFolder = LbZipFilesFolder.Content.ToString();
            string zipFile = System.IO.Path.Combine(zipFileFolder, RandomFileName + ".zip"); // M''rab zip faili asukoha ja nime

            BnConfig.IsEnabled = false;
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
                string zipPath = zipFileBrowserDialog1.FileName;
                LbSelectedZipFile.Content = zipPath;
                string extractPath = CreateExtractFolder1(zipPath);
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                IEnumerable<string> unFilteredZipFileList = CreateUnFilteredZipFileList(extractPath);
                foreach (var zipFile in unFilteredZipFileList)
                {
                    bool endsIn = (zipFile.EndsWith(".zip"));
                    if (endsIn)
                    {
                        string extractPath2 = CreateExtractFolder1(zipFile);
                        ZipFile.ExtractToDirectory(zipFile, extractPath2);
                        IEnumerable<string> unFilteredFileList = CreateUnFilteredZipFileList(extractPath2);
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
                }
            }
        }

        private IEnumerable<string> CreateUnFilteredZipFileList(string extractPath)
        {
            IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(extractPath, "*", SearchOption.AllDirectories); // Otsib ajutisest kaustast ja alamkaustadest faile
            ListUnPackedZipFiles.ItemsSource = Files; // Paigutab failid faililisti
            IEnumerable<string> unFilteredZipFileList = (IEnumerable<string>)ListUnPackedZipFiles.ItemsSource; //muudab valitud faili asukohanimetuse tekstiks
            //ListUnPackedZipFiles.ItemsSource = null; // m''rab, et alguses on faililist t[hi
            return unFilteredZipFileList;
        }

        private string CreateExtractFolder1(string zipPath)
        {
            //string temporaryRepository = System.IO.Path.GetTempPath(); //Otsib temp folderi.
            string extractFolderName = Guid.NewGuid().ToString();
            string extractPath = System.IO.Path.Combine("C:\\", "Downloaded_zip_files", extractFolderName);
            //LbSelectedZipFile.Content = extractPath;

            return extractPath;
        }


        private void ListAppSettingsFiles_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
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
                LbProcessStatus.Content = "You must select config file first";
            }

        }

        public class ConfFileInfo
        {
            private string _selectedFile;


            public string selectedFile
            {
                get { return _selectedFile; }
                set { _selectedFile = value; }
            }
        }

        private void BnConfigDownloadedAppSettings_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select directory";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = ChooseFolder(folderBrowserDialog1);
               // string selectedPath = folderBrowserDialog1.SelectedPath;
                LbTemporaryFolderZipFile.Content = selectedPath;
                LbExistingAppSettingsFilePath.Content = selectedPath; // n'itab valitud kausta
                
                string temporaryFolder = LbTemporary.Content.ToString();
                //string selectedPath = LbTemporaryFolderZipFile.Content.ToString();

                string[] folderIsEmpty = Directory.GetFiles(selectedPath);
                if (folderIsEmpty.Length == 0)
                {
                    DirectoryInfo diSource = new DirectoryInfo(LbTemporary.Content.ToString());
                    DirectoryInfo diTarget = new DirectoryInfo(ChooseFolder(folderBrowserDialog1));
                    CopyAll(diSource, diTarget);
                    string confFilePath = FindAppSettingsFile(selectedPath);

                    //LbExistingAppSettingsFilePath.Content = confFilePath;
                    //ListAppSettings(confFilePath, appSettingsPath: LbExistingAppSettingsFilePath, configSettings: LvDownloadedConfigSettings, saveChanges: BnSaveDownloadedAppSettingsChanges);
                    ObservableCollection<AppSettingsConfig> appSettingsCollection = FindConfSettings(confFilePath, statusLabel: LbDownloadedProcessStatus);
                    LvDownloadedConfigSettings.ItemsSource = appSettingsCollection;
                }
                else
                {
                    string existingConfFilePath = FindAppSettingsFile(selectedPath);
                    string downloadedConfFilePath = FindAppSettingsFile(temporaryFolder);
                    string existingConfFileName = System.IO.Path.GetFileName(path: existingConfFilePath);
                    string downloadedConfFileName = System.IO.Path.GetFileName(downloadedConfFilePath);
                    if (existingConfFileName == downloadedConfFileName)
                    {
                        string temporaryConFileDirectory = LbTemporaryComparedConfFilePath.Content.ToString();
                       // string temporaryConFileName = System.IO.Path.GetFileName(existingConfigFilePath);
                        string temporaryConFilePath = System.IO.Path.Combine(temporaryConFileDirectory, existingConfFileName);
                        File.Copy(existingConfFilePath, temporaryConFilePath);

                        //LbExistingAppSettingsFilePath.Content = existingConfFilePath;
                        LbDownloadedAppSettingsFilePath.Content = downloadedConfFilePath;
                        ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = CompareAppSettings(existingConfFilePath , downloadedConfFilePath);
                        AddRadioButtons(comparedAppSettingsCollection);
                        HideEmptyTextBox(comparedAppSettingsCollection);
                        AddThickBorder(comparedAppSettingsCollection);
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


        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }


        private void BnSaveDownloadedAppSettingsChanges_Click(object sender, RoutedEventArgs e)
        {
            //string appConfigPath = LbExistingAppSettingsFilePath.Content.ToString();
            string downloadedConfigFilePath = LbDownloadedAppSettingsFilePath.Content.ToString();
            string existingConfigFileDirectory = LbExistingAppSettingsFilePath.Content.ToString();
            //DirectoryInfo diSource = new DirectoryInfo(downloadedConfigFilePath);
            //DirectoryInfo diTarget = new DirectoryInfo(existingConfigFilePath);
            //String dirName = diSource.Directory.Name;
            string temporaryConFileDirectory = LbTemporaryComparedConfFilePath.Content.ToString();
            string temporaryConFileName = System.IO.Path.GetFileName(downloadedConfigFilePath);
            string temporaryConFilePath = System.IO.Path.Combine(temporaryConFileDirectory, temporaryConFileName);
            bool overwrite = true;
            //File.Copy(existingConfigFilePath, temporaryConFilePath);
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = CreateComparedAppSettingsDicitionary();            
            WriteSettingsToConfFile(temporaryConFilePath, appSettingsDic: appSettingsDictionary, statusLabel: LbDownloadedProcessStatus);
            File.Copy(temporaryConFilePath, System.IO.Path.Combine(existingConfigFileDirectory, temporaryConFileName), overwrite);
            //CopyAll(diSource, diTarget);
            //foreach (var file in Directory.GetFiles(DirectoryInfo diSource.ToString())
            //{
            //    File.Copy(file, System.IO.Path.Combine(existingConfigFileName, System.IO.Path.GetFileName(file)), false);
            //}
        }

        private Dictionary<string, AppSettingsConfig> CreateComparedAppSettingsDicitionary()
        {
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = LvDownloadedConfigSettings.ItemsSource as ObservableCollection<AppSettingsConfig>;
            Dictionary<string, AppSettingsConfig> newAppSettingsDictionary = new Dictionary<string, AppSettingsConfig>();
            foreach (var appSetting in comparedAppSettingsCollection)
            {
                string key = appSetting.Key;
                //AppSettingsConfig NewValue = appSetting.NewValue;
                //AppSettingsConfig ExistingValue = appSetting.ExistingValue;
                
               
                newAppSettingsDictionary.Add(key, appSetting);           
            }
            return newAppSettingsDictionary;
        }
    }
}
