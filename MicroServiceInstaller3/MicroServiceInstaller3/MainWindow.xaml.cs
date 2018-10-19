using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Transactions;
using System.Collections.ObjectModel;
using System.Configuration;
using MicroServiceInstaller3.Poco;
using System.Xml.Linq;
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
            string zipDirectory = ConfigurationManager.AppSettings["zipDirectory"];
            LbZipFilesFolder.Content = FShandler.MakeDirectorytoTemp(zipDirectory);
            string workDirectory = ConfigurationManager.AppSettings["workDirectory"];
            LbworkFilesFolder.Content = FShandler.MakeDirectorytoTemp(workDirectory);

        }
        string randomFileName = "";

        public static string ChooseFolder(FolderBrowserDialog folderBrowserDialog1, System.Windows.Controls.Label selectedFolderLabel, System.Windows.Controls.Button savebutton)
        {
            string selectedPath = folderBrowserDialog1.SelectedPath; // Loob muutuja, mis vastab valitud kaustale
            selectedFolderLabel.Content = selectedPath;// M''rab, kuhu kuvatakse valitud kausta sisu
            if (selectedFolderLabel.HasContent)
            {
                savebutton.IsEnabled = true;
            }
            return selectedPath;
        }

        private void BSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select directory";
            folderBrowserDialog1.ShowNewFolderButton = false;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = ChooseFolder(folderBrowserDialog1, selectedFolderLabel: LbSelectedFolder, savebutton: BnZip);
                string workFilesFolderPath = LbworkFilesFolder.Content.ToString();
                //FShandler.CopyAll(selectedPath, workFilesFolderPath);
                FShandler.DirectoryCopy(selectedPath, workFilesFolderPath, copySubDirs: true);
                IEnumerable<string> unFilteredFileList = CreateUnFilteredFileList(workFilesFolderPath);
                //string value = 
                FilterFileList(unFilteredFileList);
                //if (value != null)
                //{
                    
                //}
                FShandler.CreateMetaDataFile(selectedPath, workFilesFolderPath);
            }
        }

        public void FilterFileList(IEnumerable<string> unFilteredFileList)
        {
            foreach (var file in unFilteredFileList) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
            {
                if (!File.Exists(file))
                {
                    continue; // kui fail eksisteerib, j'tkab
                }
                else
                {
                    bool endsIn = (file.EndsWith(".pdb") || file.Contains("/deps/") || file.Contains(".vshost.")); // kui faili asukohanimetus sisaldab j'rgmis v''rtusi
                    if (endsIn)
                    {
                        File.Delete(file);
                        //return null;// kustutab faili
                    }
                    else
                    {
                        ListFiles.Items.Add($"{file}");
                        //return file;                   
                    }
                }
            }
            //return null;
        }

        private IEnumerable<string> CreateUnFilteredFileList(string temporaryFolder)
        {
            ListFiles.ItemsSource = null;
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
            LbProcessStatus.Content = "";
            string confFilePath = LbTemporary.Content.ToString();
            try
            {
                ObservableCollection<AppSettingsConfig> appsettingsCollection = ConfFileHandler.FindAppSettings(confFilePath);
                LvUploadedConfigSettings.ItemsSource = appsettingsCollection;
                ObservableCollection<ConnectionStrings> ConnectionStringsCollection = ConfFileHandler.FindConnectionsStrings(confFilePath);
                LvUploadedConnectionSettings.ItemsSource = ConnectionStringsCollection;
            }
            catch (Exception error)
            {
                LbProcessStatus.Content = "This file does not consist ConfSettings, please select another file"  + error.Message;
            }
            BnSaveChanges.IsEnabled = true;
        }

        private void BnSaveChanges_Click(object sender, RoutedEventArgs e)
        {        
            string appConfigPath = LbworkFilesFolder.Content.ToString();
            string selectedPath = LbTemporary.Content.ToString();         
            ObservableCollection<AppSettingsConfig> modifiedAppSettings = LvUploadedConfigSettings.ItemsSource as ObservableCollection<AppSettingsConfig>;
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = ConfFileHandler.ReadModifiedAppSettings( modifiedAppSettings);
            //appConfigPath = appSettingsPath.Content.ToString();
            try
            {
                ConfFileHandler.WriteSettingsToConfFile(selectedPath, appSettingsDic: appSettingsDictionary);
                LbProcessStatus.Content = "Changes saved ";
            }
            catch (Exception error)
            {
                LbProcessStatus.Content = error.Message;
            }
            ObservableCollection<ConnectionStrings> connectionStringsCollection = LvUploadedConnectionSettings.ItemsSource as ObservableCollection<ConnectionStrings>;
            //Dictionary<string, ConnectionStrings> connectionStringsDictionary = ConfFileHandler.CreateConnectionStringsDicitionary(connectionStringsCollection);
            try
            {
                ConfFileHandler.WriteConnectionStringstoConFile(selectedPath, connectionStringsCollection);

            }
             catch (Exception error)          
            {
                LbProcessStatus.Content = error.Message;
            }
        }

        private void BnZip_Click(object sender, RoutedEventArgs e)
        {
            string InitialsFilesFolder = LbworkFilesFolder.Content.ToString();
            string zipFileFolder = LbZipFilesFolder.Content.ToString();
            randomFileName = Guid.NewGuid().ToString();
            string zipFile = System.IO.Path.Combine(zipFileFolder, randomFileName + ".zip"); // M''rab zip faili asukoha ja nime

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
                LvUploadedConnectionSettings.ItemsSource = "";
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
                string finalZipPath = ConfigurationManager.AppSettings["finalZipDirectory"];
                FShandler.MakeDirectory(finalZipPath);
                //LbFinalZipFolder.Content = finalZipPath;

                string finalZipFileName = System.IO.Path.Combine(finalZipPath, "final.zip");
                //if (File.Exists(finalZipFileName))
                //{
                //    File.Delete(finalZipFileName);
                //}
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
                string extractFolderPath = FShandler.MakeRandomDirectorytoTemp();
                //string extractFolderPath = ConfigurationManager.AppSettings["extractFolderPath"];
                //string extractDirectory = FShandler.CreateExtractFolder(extractFolderPath);
                ZipFile.ExtractToDirectory(zipFileName, extractFolderPath);
                IEnumerable<string> unFilteredZipFileList = CreateUnFilteredZipFileList(extractFolderPath);
                foreach (var zipFile in unFilteredZipFileList)
                {
                    bool endsIn = (zipFile.EndsWith(".zip"));
                    if (endsIn)
                    {
                        string extractFolderPath2 = FShandler.MakeRandomDirectorytoTemp();
                        ZipFile.ExtractToDirectory(zipFile, extractFolderPath2);
                        IEnumerable<string> unFilteredFileList = CreateUnFilteredZipFileList(extractFolderPath2);
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

        private void BnConfigDownloadedAppSettings_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select directory";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = ChooseFolder(folderBrowserDialog1, selectedFolderLabel: LbExistingAppSettingsFilePath , savebutton: BnSaveDownloadedAppSettingsChanges );
                LbTemporaryFolderZipFile.Content = selectedPath;
                LbExistingAppSettingsFilePath.Content = selectedPath; // n'itab valitud kausta Seda rida pole vaja, toimub chooseFolderi funktsioonis!!!!
                string temporaryFolder = LbTemporary.Content.ToString();
                string[] folderIsEmpty = Directory.GetFiles(selectedPath);
                ObservableCollection<AppSettingsConfig> appSettingsCollection = null;
                ObservableCollection<ConnectionStrings> connectionStringsCollection = null;
                if (folderIsEmpty.Length == 0)
                {
                    //DirectoryInfo diSource = new DirectoryInfo(temporaryFolder);
                    //DirectoryInfo diTarget = new DirectoryInfo(selectedPath);
                    FShandler.DirectoryCopy(temporaryFolder, selectedPath, copySubDirs: true);
                    //FShandler.CopyAll(diSource, diTarget);
                    string confFilePath = ConfFileHandler.FindAppSettingsFile(selectedPath);
                    try
                    {
                    appSettingsCollection = ConfFileHandler.FindAppSettings(confFilePath);
                    connectionStringsCollection = ConfFileHandler.FindConnectionsStrings(confFilePath);

                    }
                    catch (Exception error)
                    {
                        LbDownloadedProcessStatus.Content = "This file does not consist appsettings, please select another file" + error.Message;
                    }
                    LvDownloadedConfigSettings.ItemsSource = appSettingsCollection;
                    LvDownLoadedConnectionSettings.ItemsSource = connectionStringsCollection;
                    AddRadioButtons(appSettingsCollection);
                    AddRadioButtons(connectionStringsCollection);
                    HideEmptyTextBox(appSettingsCollection);
                  //  HideEmptyTextBox(connectionStringsCollection);
                    BnSaveDownloadedAppSettingsChanges.IsEnabled = true;
                }
                else
                {
                    string existingConfFilePath = ConfFileHandler.FindAppSettingsFile(selectedPath);
                    string downloadedConfFilePath = ConfFileHandler.FindAppSettingsFile(temporaryFolder);
                    string existingConfFileName = System.IO.Path.GetFileName(path: existingConfFilePath);
                    string downloadedConfFileName = System.IO.Path.GetFileName(downloadedConfFilePath);
                    ObservableCollection < AppSettingsConfig > comparedAppSettingsCollection = null;
                    if (existingConfFileName == downloadedConfFileName)
                    {
                        LbDownloadedAppSettingsFilePath.Content = downloadedConfFilePath;
                        try
                        {
                            comparedAppSettingsCollection = ConfFileHandler.CompareAppSettings( existingConfFilePath, downloadedConfFilePath);
                        }
                        catch (Exception error)
                        {
                            LbStatus.Content = "This file dont consist appsettings" + error.Message; // see label on vale
                        }
                        LvDownloadedConfigSettings.ItemsSource = comparedAppSettingsCollection;
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

        //private void AddThickBorder(ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection)
        //{
        //    foreach (var item in comparedAppSettingsCollection)
        //    {
        //        if (item.RbExistingValue == true)
        //        {
        //            item.TbExistigValueBorder = new Thickness(3);
        //        }
        //        if (item.RbNewValue == true)
        //        {
        //            item.TbNewValueBorder = new Thickness(3);
        //        }
        //    }
        //}

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

        private void AddRadioButtons(IEnumerable appSettings)
        {
            foreach (var it in appSettings)
            {
                //if(typeof(ConnectionStrings)== it.GetType()) niimoodi 'ra tee
                Poco.SettingsBase item = it as Poco.SettingsBase;
                if (!string.IsNullOrEmpty("test")) //
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
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = LvDownloadedConfigSettings.ItemsSource as ObservableCollection<AppSettingsConfig>;
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = ConfFileHandler.CreateComparedAppSettingsDicitionary(comparedAppSettingsCollection);
            try
            {
                ConfFileHandler.WriteSettingsToConfFile(existingConfFilePath, appSettingsDic: appSettingsDictionary);
                LbDownloadedProcessStatus.Content = "Changes saved ";             
            }
            catch (Exception error)
            {
                LbDownloadedProcessStatus.Content = error.Message;
            }
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
    }
}
