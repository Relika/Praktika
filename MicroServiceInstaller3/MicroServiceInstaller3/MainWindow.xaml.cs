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
                string temporaryFolder = CreateTemopraryFolder(selectedPath);

                DirectoryCopy(selectedPath, temporaryFolder,copySubDirs: true);
                IEnumerable<string> unFilteredFileList = CreateUnFilteredFileList(temporaryFolder);
                FilterFileList(unFilteredFileList);
                CreateMetaDataFile(selectedPath, temporaryFolder);
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

        private string CreateTemopraryFolder(string selectedPath)
        {
            string temporaryRepository = System.IO.Path.GetTempPath(); //Otsib temp folderi.
            string temporaryFolder = System.IO.Path.Combine(temporaryRepository, "tempDirectory");

            LbTemporaryFolder.Content = temporaryFolder;
            return temporaryFolder;
        }

        private string ChooseFolder(FolderBrowserDialog folderBrowserDialog1)
        {
            string selectedPath = folderBrowserDialog1.SelectedPath; // Loob muutuja, mis vastab valitud kaustale
            LbSelectedFolder.Content = "Selected folder: "+selectedPath;// M''rab, kuhu kuvatakse valitud kausta sisu
            if (LbSelectedFolder.HasContent)
            {
                BnConfig.IsEnabled = true;
                BnZip.IsEnabled = true;
            }
            return selectedPath;
        }

        private void BnConfig_Click(object sender, RoutedEventArgs e)
        {
            string temporaryFolder = LbTemporaryFolder.Content.ToString();
            foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(temporaryFolder, "*", SearchOption.AllDirectories)) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
            {
                if (!File.Exists(fileSystemEntry)) continue; // kui fail ei eksisteeri, j'tkab

                bool endsIn = (fileSystemEntry.EndsWith(".exe.config"));

                if (endsIn)
                {
                    LbAppSettingsFilePath.Content = fileSystemEntry;
                    ObservableCollection<AppSettingsConfig> appSettingsDictionary = FindConfSettings(fileSystemEntry);
                    LvConfigSettings.ItemsSource = appSettingsDictionary;
                    if (LbAppSettingsFilePath.HasContent)
                    {
                        BnSaveChanges.IsEnabled = true;
                    }
                }
            }
        }

        private ObservableCollection<AppSettingsConfig> FindConfSettings(string fileSystemEntry)
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
                    appSetting.Value = (string)item.Attribute("value");
                    appSettingsCollection.Add(appSetting);
                }
            }
            catch (Exception error)
            {
                LbProcessStatus.Content = error.Message;
            }
            return appSettingsCollection;
        }

        public class AppSettingsConfig
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private void BnSaveChanges_Click(object sender, RoutedEventArgs e)
        {

            var ModifiedAppSettings = LvConfigSettings.ItemsSource;
            Dictionary<string, string> appSettingsDictionary = new Dictionary<string, string>();

            foreach (var item in ModifiedAppSettings)
            {
                AppSettingsConfig appSetting = item as AppSettingsConfig;
                appSettingsDictionary.Add(appSetting.Key, appSetting.Value);
            }
            string appConfigPath = LbAppSettingsFilePath.Content.ToString();

            WriteSettingsToConfFile(appConfigPath, appSettingsDic:appSettingsDictionary);
        }

        private void WriteSettingsToConfFile(string appConfigPath, Dictionary<string, string> appSettingsDic)
        {
            try
            {
                var doc = XDocument.Load(appConfigPath);
                var elements = doc.Descendants("appSettings").Elements();

                foreach (var item in elements)
                {
                    string key = (string)item.Attribute("key");

                    if (appSettingsDic.ContainsKey(key))
                    {
                        item.Attribute("value").Value = appSettingsDic[key];
                    }
                }
                doc.Save(appConfigPath);
                LbProcessStatus.Content = "Changes saved";             
            }
            catch (Exception error)
            {
                LbProcessStatus.Content = error.Message;
            }
            
        }

        private void BnZip_Click(object sender, RoutedEventArgs e)
        {
            string temporaryFolder = LbTemporaryFolder.Content.ToString();
            string zipPath = System.IO.Path.Combine(temporaryFolder,"..", RandomFileName+".zip"); // M''rab zip faili asukoha ja nime

            BnConfig.IsEnabled = false;
            using (var scope = new TransactionScope())
            {
                File.Delete(zipPath); // kustutab faili, mis asub sellel aadressil

                ZipFile.CreateFromDirectory(temporaryFolder, zipPath); // loob zip faili

                bool existItem = false;
                foreach (var value in ListZipFiles.Items) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
                {
                    string ZipFilePath = value.ToString();
                    existItem = (ZipFilePath.Equals(zipPath)); // kui on olemas sama asukohanimetusega fail
                    if (existItem)
                    {                   
                        break;
                    }
                }

                if (!existItem)
                {
                    ListZipFiles.Items.Add($"{zipPath}");
                }

                ListFiles.Items.Clear(); // eemaldab listis olevad asukohakirjed
                LbSelectedFolder.Content = ""; // eemaldab valitud algse kataloogi asukoha kirje.
                LbAppSettingsFilePath.Content = "";
                LvConfigSettings.ItemsSource = "";
                LbProcessStatus.Content = "";

                if (ListZipFiles.HasItems)
                {
                    BnFinishandZip.IsEnabled = true;
                }
                scope.Complete();
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            else
            {
                Directory.Delete(destDirName, true);
                Directory.CreateDirectory(destDirName);
            }
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void BnFinishandZip_Click(object sender, RoutedEventArgs e)
        {
            BnZip.IsEnabled = false;
            using (var scope = new TransactionScope())
            {
                string finalZipLocation = System.IO.Path.Combine("C:\\", "finalZip");
                Directory.CreateDirectory(finalZipLocation);

                string finalLocation = null;
                foreach (var entry in ListZipFiles.Items) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
                {
                    string ZipFilePath = entry.ToString();
                    string zipFileName = System.IO.Path.GetFileName(ZipFilePath);

                    finalLocation = System.IO.Path.Combine(finalZipLocation, zipFileName);

                    File.Copy(ZipFilePath, finalLocation);
                }

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
                if (LbSelectedZipFile.HasContent)
                {
                    BnConfigDownloadedAppSettings.IsEnabled = true;
                }
                string extractPath = CreateExtractFolder1(zipPath);
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                //string selectedPath = ChooseFolder(folderBrowserDialog1);
                ////string extractFolder = CreateExtractFolder1(selectedPath);

                // DirectoryCopy(selectedPath, temporaryFolder, copySubDirs: true);
                IEnumerable<string> ZipFileList = CreateZipFileList(extractPath);
                //FilterFileList(unFilteredFileList);
                //CreateMetaDataFile(selectedPath, temporaryFolder);
            }
        }

        private IEnumerable<string> CreateZipFileList(string extractPath)
        {
            IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(extractPath,"*", SearchOption.AllDirectories); // Otsib ajutisest kaustast ja alamkaustadest faile
            ListUnPackedZipFiles.ItemsSource = Files; // Paigutab failid faililisti
            IEnumerable<string> ZipFileList = (IEnumerable<string>)ListUnPackedZipFiles.ItemsSource; //muudab valitud faili asukohanimetuse tekstiks
            //ListUnPackedZipFiles.ItemsSource = null; // m''rab, et alguses on faililist t[hi
            return ZipFileList;
        }

        private string CreateExtractFolder1(string zipPath)
        {
            //string temporaryRepository = System.IO.Path.GetTempPath(); //Otsib temp folderi.
            string extractFolderName = Guid.NewGuid().ToString();
            string extractPath = System.IO.Path.Combine("C:\\", "Downloaded zip files" , extractFolderName);
            LbSelectedZipFile.Content = extractPath;
         
            return extractPath;
        }

        private void BnConfigDownloadedAppSettings_Click(object sender, RoutedEventArgs e)
        {
            string temporaryFolder = LbSelectedZipFile.Content.ToString();
            foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(temporaryFolder, "*", SearchOption.AllDirectories)) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
            {
                if (!File.Exists(fileSystemEntry)) continue; // kui fail ei eksisteeri, j'tkab

                bool endsIn = (fileSystemEntry.EndsWith(".exe.config"));

                if (endsIn)
                {
                    LbDownloadedAppSettingsFilePath.Content = fileSystemEntry;
                    ObservableCollection<AppSettingsConfig> appSettingsDictionary = FindConfSettings(fileSystemEntry);
                    LvDowloadedConfigSettings.ItemsSource = appSettingsDictionary;
                    if (LbDownloadedAppSettingsFilePath.HasContent)
                    {
                        BnSaveChanges.IsEnabled = true;
                    }
                }
            }
        }

        //private ObservableCollection<AppSettingsConfig> FindConfSettings(string fileSystemEntry)
        //{
        //    ObservableCollection<AppSettingsConfig> appSettingsCollection = new ObservableCollection<AppSettingsConfig>();
        //    try
        //    {
        //        var doc = XDocument.Load(fileSystemEntry);
        //        var elements = doc.Descendants("appSettings").Elements();

        //        foreach (var item in elements)
        //        {
        //            AppSettingsConfig appSetting = new AppSettingsConfig();
        //            appSetting.Key = (string)item.Attribute("key");
        //            appSetting.Value = (string)item.Attribute("value");
        //            appSettingsCollection.Add(appSetting);
        //        }
        //    }
        //    catch (Exception error)
        //    {
        //        LbProcessStatus.Content = error.Message;
        //    }
        //    return appSettingsCollection;
        //}



        private void BnSaveDownloadedAppSettingsChanges_Click(object sender, RoutedEventArgs e)
        {

        }

        //public class AppSettingsConfig
        //{
        //    public string Key { get; set; }
        //    public string Value { get; set; }
        //}

        //private void BnSaveChanges_Click(object sender, RoutedEventArgs e)
        //{

        //    var ModifiedAppSettings = LvConfigSettings.ItemsSource;
        //    Dictionary<string, string> appSettingsDictionary = new Dictionary<string, string>();

        //    foreach (var item in ModifiedAppSettings)
        //    {
        //        AppSettingsConfig appSetting = item as AppSettingsConfig;
        //        appSettingsDictionary.Add(appSetting.Key, appSetting.Value);
        //    }
        //    string appConfigPath = LbAppSettingsFilePath.Content.ToString();

        //    WriteSettingsToConfFile(appConfigPath, appSettingsDic: appSettingsDictionary);
        //}

        //private void WriteSettingsToConfFile(string appConfigPath, Dictionary<string, string> appSettingsDic)
        //{
        //    try
        //    {
        //        var doc = XDocument.Load(appConfigPath);
        //        var elements = doc.Descendants("appSettings").Elements();

        //        foreach (var item in elements)
        //        {
        //            string key = (string)item.Attribute("key");

        //            if (appSettingsDic.ContainsKey(key))
        //            {
        //                item.Attribute("value").Value = appSettingsDic[key];
        //            }
        //        }
        //        doc.Save(appConfigPath);
        //        LbProcessStatus.Content = "Changes saved";
        //    }
        //    catch (Exception error)
        //    {
        //        LbProcessStatus.Content = error.Message;
        //    }

        //}
    }
}
