using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Path = System.IO.Path;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using CommonLibary.Handlers;
using System.IO.Compression;
using CommonLibary.Poco;
using System.Collections.ObjectModel;
using System.Collections;
using MicroServiceInstaller3;
using System.Reflection;
using System.Resources;
using System.Linq;

namespace ServiceInstallClient
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



        private void ListAppSettingsFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LbDownloadedAppSettingsFilePath.Content = "";
            LvDownLoadedConnectionSettings.ItemsSource = "";
            LvDownloadedConfigSettings.ItemsSource = "";
            LbExistingAppSettingsFilePath.Content = "";
            if (ListAppSettingsFiles.SelectedIndex >= 0)
            {
                BnConfigDownloadedAppSettings.IsEnabled = true;
                LbTemporary.Content = ListAppSettingsFiles.SelectedItem;
                string selectedFile = LbTemporary.Content.ToString();
                string filePath = Path.GetDirectoryName(selectedFile);
                LbTemporary.Content = filePath;
                LbDownloadedAppSettingsFilePath.Content = selectedFile;
            }
            else
            {
                LbDownloadedProcessStatus.Content = "You have to select config file first";
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
                    TbGuide.Text = "Please select conffile from the list to config settings";
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
            TbGuide.Text = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select directory";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = FShandler.ChooseFolder(folderBrowserDialog1, selectedFolderLabel: LbExistingAppSettingsFilePath, savebutton: BnSaveDownloadedAppSettingsChanges);
                LbTemporaryFolderZipFile.Content = selectedPath;
                LbExistingAppSettingsFilePath.Content = selectedPath; // n'itab valitud kausta Seda rida pole vaja, toimub chooseFolderi funktsioonis!!!!
                string temporaryFolder = LbTemporary.Content.ToString();
                string[] folderIsEmpty = Directory.GetFiles(selectedPath);
                ObservableCollection<AppSettingsConfig> appSettingsCollection = null;
                ObservableCollection<ConnectionStrings> connectionStringsCollection = null;
                if (folderIsEmpty.Length == 0)
                {
                    FShandler.DirectoryCopy(temporaryFolder, selectedPath, copySubDirs: true);
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
                    BnSaveDownloadedAppSettingsChanges.IsEnabled = true;
                }
                else
                {
                    string existingConfFilePath = ConfFileHandler.FindAppSettingsFile(selectedPath);
                    string downloadedConfFilePath = ConfFileHandler.FindAppSettingsFile(temporaryFolder);
                    string existingConfFileName = System.IO.Path.GetFileName(path: existingConfFilePath);
                    string downloadedConfFileName = System.IO.Path.GetFileName(downloadedConfFilePath);
                    ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = null;
                    ObservableCollection<ConnectionStrings> comparedConnectionStringCollection = null;
                    if (existingConfFileName == downloadedConfFileName)
                    {
                        LbDownloadedAppSettingsFilePath.Content = downloadedConfFilePath;
                        try
                        {
                            comparedAppSettingsCollection = ConfFileHandler.CompareAppSettings(existingConfFilePath, downloadedConfFilePath);
                            comparedConnectionStringCollection = ConfFileHandler.CompareConnectionStrings(existingConfFilePath, downloadedConfFilePath);
                            //compare connectionsStrings
                        }
                        catch (Exception error)
                        {
                            TbGuide.Text = "This file dont consist appsettings or connectinStrings" + error.Message;
                        }
                        LvDownloadedConfigSettings.ItemsSource = comparedAppSettingsCollection;
                        LvDownLoadedConnectionSettings.ItemsSource = comparedConnectionStringCollection;
                        AddRadioButtons(comparedAppSettingsCollection);
                        AddRadioButtons(comparedConnectionStringCollection);
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

        private void AddRadioButtons(IEnumerable appSettings)
        {
            foreach (var it in appSettings)
            {
                //if(typeof(ConnectionStrings)== it.GetType()) niimoodi 'ra tee
                CommonLibary.Poco.SettingsBase item = it as CommonLibary.Poco.SettingsBase;

                if (item.IsValueExist) //
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
            string existingConfFilePath = Path.Combine(existingConfigFileDirectory, Path.GetFileName(downloadedConfigFilePath));
            ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = LvDownloadedConfigSettings.ItemsSource as ObservableCollection<AppSettingsConfig>;
            ObservableCollection<ConnectionStrings> comparedConnectinStringsCollection = LvDownLoadedConnectionSettings.ItemsSource as ObservableCollection<ConnectionStrings>;
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = ConfFileHandler.CreateComparedAppSettingsDicitionary(comparedAppSettingsCollection);
            Dictionary<string, ConnectionStrings> connectionStringsDictionary = ConfFileHandler.CreateComparedConnectionStringsDicitionary(comparedConnectinStringsCollection);
            string serviceName = ConfFileHandler.GetServiceName(downloadedConfigFilePath);
            try
            {
                ServiceState serviceStatusbefore = ServiceInstaller.GetServiceStatus(serviceName);
                if (serviceStatusbefore == ServiceState.Running)
                {
                    ServiceInstaller.StopService(serviceName);
                    ConfFileHandler.WriteSettingsToConfFile(existingConfFilePath, appSettingsDic: appSettingsDictionary);
                    ConfFileHandler.WriteConnectionStringstoConFile(existingConfFilePath, connectionStringsDic: connectionStringsDictionary);
                    ServiceInstaller.StartService(serviceName);
                }
                if (serviceStatusbefore == ServiceState.NotFound)
                {
                    ConfFileHandler.WriteSettingsToConfFile(existingConfFilePath, appSettingsDic: appSettingsDictionary);
                    ConfFileHandler.WriteConnectionStringstoConFile(existingConfFilePath, connectionStringsDic: connectionStringsDictionary);
                    ServiceInstaller.InstallAndStart(serviceName, serviceName, downloadedConfigFilePath);
                }
                else
                {
                    ConfFileHandler.WriteSettingsToConfFile(existingConfFilePath, appSettingsDic: appSettingsDictionary);
                    ConfFileHandler.WriteConnectionStringstoConFile(existingConfFilePath, connectionStringsDic: connectionStringsDictionary);
                    ServiceInstaller.StartService(serviceName);
                }

                ServiceState serviceStatusafter = ServiceInstaller.GetServiceStatus(serviceName);
                LbDownloadedProcessStatus.Content = "Changes saved, service status: " + serviceStatusafter;
                LvDownLoadedConnectionSettings.ItemsSource = "";
                LvDownloadedConfigSettings.ItemsSource = "";
                LbExistingAppSettingsFilePath.Content = "";
                BnSaveDownloadedAppSettingsChanges.IsEnabled = false;
            }
            catch (Exception error)
            {
                LbDownloadedProcessStatus.Content = error.Message;
            }
        }

        private void BnCloseDownload_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BTestSelectZipFile_Click(object sender, RoutedEventArgs e)
        {
            GetResourcesUnder("Resources");
        }
        public void GetResourcesUnder(string folder)
        {
            folder = folder.ToLower() + "/";

            var assembly = Assembly.GetCallingAssembly();
            var resourcesName = assembly.GetName().Name + ".g.resources";
            var stream = assembly.GetManifestResourceStream(resourcesName);
            var resourceReader = new ResourceReader(stream);
            string resourceType;
            byte[] resourceData;

           // resourceReader.GetResourceData("Debug.zip", out resourceType, out resourceData);
            resourceReader.GetResourceData("Test", out resourceType, out resourceData);
            var resources =
                from p in resourceReader.OfType<DictionaryEntry>()
                let theme = (string)p.Key
                where theme.StartsWith(folder)
                select theme.Substring(folder.Length);

            foreach (var item in resources.ToArray())
            {

                ListAppSettingsFiles.Items.Add(item);
            }
            //return resources.ToArray();
        }
    }
}
