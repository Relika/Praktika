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
using CommonLibary.Poco;
using CommonLibary.Handlers;

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
            string finalZipDirectory = ConfigurationManager.AppSettings["finalZipDirectory"];
            LbFinalZipFolder.Content = FShandler.MakeDirectorytoTemp(finalZipDirectory);
        }
        string randomFileName = "";


        private void BSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select directory";
            folderBrowserDialog1.ShowNewFolderButton = false;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath =FShandler.ChooseFolder(folderBrowserDialog1, selectedFolderLabel: LbSelectedFolder, savebutton: BnZip);
                string workFilesFolderPath = LbworkFilesFolder.Content.ToString();
                FShandler.DirectoryCopy(selectedPath, workFilesFolderPath, copySubDirs: true);
                IEnumerable<string> unFilteredFileList = CreateUnFilteredFileList(workFilesFolderPath);
                FilterFileList(unFilteredFileList);
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
                    }
                    else
                    {
                        ListFiles.Items.Add($"{file}");                
                    }
                }
            }
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
                LbProcessStatus.Content = "";
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
            Dictionary<string, ConnectionStrings> connectionStringsDictionary = ConfFileHandler.CreateConnectionStringsDicitionary(connectionStringsCollection);
            try
            {
                ConfFileHandler.WriteConnectionStringstoConFile(selectedPath, connectionStringsDic: connectionStringsDictionary);
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
            BnZip.IsEnabled = false;
        }

        private void BnFinishandZip_Click(object sender, RoutedEventArgs e)
        {
            using (var scope = new TransactionScope())
            {
                //Read zipFiles
                string zipLocation = LbZipFilesFolder.Content.ToString();
                // Create finalZip
                string temporaryDirectory = LbFinalZipFolder.Content.ToString();
                string finalZipFileName = "final.zip";
                string finalZipFilePath = System.IO.Path.Combine(temporaryDirectory, finalZipFileName);
                ZipFile.CreateFromDirectory(zipLocation, finalZipFilePath);
                // Save finalZip to ServiceInstallClient.exe resourses
                byte[] finalZipBytes = File.ReadAllBytes(finalZipFilePath);
                string exeFilePath = @"ServiceInstallClient.exe";
                string finalExe = "start.exe";
                ResourceHandler.AddResource(exeFilePath, finalZipFileName, finalZipBytes, finalExe);
                LbStatus.Content = "Zip file is created successfully: " + finalExe;
                BnFinishandZip.IsEnabled = false;
                ListZipFiles.Items.Clear(); // eemaldab listis olevad valitud zip failide asukohakirjed
                scope.Complete();            
            }
        }

        private void BnCloseUpload_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

    }
}
