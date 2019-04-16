using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Path = System.IO.Path;
using System.Windows.Forms;
using CommonLibary.Handlers;
using System.IO.Compression;
using CommonLibary.Poco;
using System.Collections.ObjectModel;
using System.Collections;
using MicroServiceInstaller3;
using System.Transactions;
using System.ServiceProcess;
using System.Linq;
using System.Configuration.Install;
using System.ComponentModel;
using System.Diagnostics;

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
            LbDownloadedProcessStatus.Content = "";
            
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

        private void BTestSelectZipFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string temporaryFolder = System.IO.Path.GetDirectoryName(path);
                string zipFile= ConfFileHandler.FindZipFile(temporaryFolder);
                string temporaryDirectory = FShandler.MakeRandomDirectorytoTemp();
                ZipFile.ExtractToDirectory(zipFile, temporaryDirectory);
                // controls if there are more zip-files
                if (ConfFileHandler.FindZipFile(temporaryDirectory) == ""){
                    IEnumerable<string> unFilteredFileList = CreateUnFilteredFileList(temporaryDirectory);
                    FilterFileList(unFilteredFileList);
                } else{
                    IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(temporaryDirectory, "*", SearchOption.AllDirectories);
                    // lists found files
                    ListUnPackedZipFiles.ItemsSource = Files;
                    foreach (object item in ListUnPackedZipFiles.ItemsSource)
                    {
                        string temporaryDirectory2 = FShandler.MakeRandomDirectorytoTemp();
                        ZipFile.ExtractToDirectory(item.ToString(), temporaryDirectory2);
                        IEnumerable<string> unFilteredFileList = CreateUnFilteredFileList(temporaryDirectory2);
                        FilterFileList(unFilteredFileList);
                    }
                }
            }
            catch (Exception error)
            {
                LbProcessStatus.Content = "No zipfile found "+error.Message;
            }
            finally
            {
                BTestSelectZipFile.IsEnabled = false;
            }
        }
        /// <summary>
        /// Filters listed files
        /// </summary>
        /// <param name="unFilteredFileList">List of files need to be filtered</param>
         private void FilterFileList(IEnumerable<string> unFilteredFileList)
        {
            foreach (var value in unFilteredFileList)
            {
                if (!File.Exists(value)) continue;
                bool endsIn = (value.EndsWith(".exe.config"));
                if (endsIn)
                {
                    ListAppSettingsFiles.Items.Add($"{value}");
                    TbGuide.Text = "Please select conffile from the list to config settings";                
                }
            }
        }
        /// <summary>
        /// Creates list of files found from selected directory and subdirectories
        /// </summary>
        /// <param name="directoryPath">selected directory</param>
        /// <returns>Returns list of files</returns>
        private IEnumerable<string> CreateUnFilteredFileList(string directoryPath)
        {
            IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(directoryPath, "*", SearchOption.AllDirectories);
            ListUnPackedZipFiles.ItemsSource = Files;
            IEnumerable<string> unFilteredZipFileList = (IEnumerable<string>)ListUnPackedZipFiles.ItemsSource; //changes filenames to string
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
                LbExistingAppSettingsFilePath.Content = selectedPath;
                string temporaryFolder = LbTemporary.Content.ToString();
                string[] folderIsEmpty = Directory.GetFiles(selectedPath);
                ObservableCollection<AppSettingsConfig> appSettingsCollection = null;
                ObservableCollection<ConnectionStrings> connectionStringsCollection = null;
                if (folderIsEmpty.Length == 0)//If selected folder is empty
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
                } else {// If selected folder is not empty
                    string existingConfFilePath = ConfFileHandler.FindAppSettingsFile(selectedPath);
                    string downloadedConfFilePath = ConfFileHandler.FindAppSettingsFile(temporaryFolder);
                    string existingConfFileName = Path.GetFileName(path: existingConfFilePath);
                    string downloadedConfFileName = Path.GetFileName(downloadedConfFilePath);
                    ObservableCollection<AppSettingsConfig> comparedAppSettingsCollection = null;
                    ObservableCollection<ConnectionStrings> comparedConnectionStringCollection = null;
                    if (existingConfFileName == downloadedConfFileName)// If confile names match.
                    {
                        LbDownloadedAppSettingsFilePath.Content = downloadedConfFilePath;
                        try
                        {
                            // compare appSetting and connectionsStrings
                            comparedAppSettingsCollection = ConfFileHandler.CompareAppSettings(existingConfFilePath, downloadedConfFilePath);
                            comparedConnectionStringCollection = ConfFileHandler.CompareConnectionStrings(existingConfFilePath, downloadedConfFilePath);
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
                LbLogFilePath.Content = FShandler.CreateLogFile(selectedPath);
            }
        }
        /// <summary>
        /// Adds radio buttons to listed elements
        /// </summary>
        /// <param name="elementsList">List of elements</param>
        private void AddRadioButtons(IEnumerable elementsList)
        {
            foreach (var it in elementsList)
            {
                SettingsBase item = it as SettingsBase;

                if (item.IsValueExist)
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
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = ConfFileHandler.CreateAppSettingsDicitionary(comparedAppSettingsCollection);
            Dictionary<string, ConnectionStrings> connectionStringsDictionary = ConfFileHandler.CreateComparedConnectionStringsDicitionary(comparedConnectinStringsCollection);
            string serviceName = ConfFileHandler.GetServiceName(downloadedConfigFilePath);
            LbServiceName.Content = serviceName;
            string serviceFilePath = Path.Combine(existingConfigFileDirectory, serviceName + ".exe");
            LogHandler.WriteLogMessage(LbLogFilePath.Content.ToString(), "serviceName: " + serviceName);
            using (TransactionScope scope = new TransactionScope())
            { 
                try
                {
                    ServiceController service = new ServiceController(serviceName);
                    bool isExists = DoesServiceExist(serviceName);
                    if (isExists)
                    {
                        LogHandler.WriteLogMessage(LbLogFilePath.Content.ToString(), "ServiceStatusBefore: " + service.Status);
                        if (service.Status == ServiceControllerStatus.Stopped)
                        {
                            ConfFileHandler.WriteSettingsToConfFile(existingConfFilePath, appSettingsDic: appSettingsDictionary);
                            ConfFileHandler.WriteConnectionStringstoConFile(existingConfFilePath, connectionStringsDic: connectionStringsDictionary);
                            service.Start();
                            var timeout = new TimeSpan(0, 0, 5);
                            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                            LogHandler.WriteLogMessage(LbLogFilePath.Content.ToString(), "ServiceStatus: " + service.Status);
                        }
                        else
                        {
                            service.Stop();
                            var timeout = new TimeSpan(0, 0, 5); // 5seconds
                            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                            ConfFileHandler.WriteSettingsToConfFile(existingConfFilePath, appSettingsDic: appSettingsDictionary);
                            ConfFileHandler.WriteConnectionStringstoConFile(existingConfFilePath, connectionStringsDic: connectionStringsDictionary);
                            service.Start();
                            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                            LogHandler.WriteLogMessage(LbLogFilePath.Content.ToString(), "ServiceStatus: " + service.Status);
                        }
                    }
                    else
                    {
                        ConfFileHandler.WriteSettingsToConfFile(existingConfFilePath, appSettingsDic: appSettingsDictionary);
                        ConfFileHandler.WriteConnectionStringstoConFile(existingConfFilePath, connectionStringsDic: connectionStringsDictionary);
                        string exeFilePath = ConfFileHandler.GetExeFileName(existingConfFilePath);
                        CreateService(serviceName, serviceFilePath);
                        bool exists = FindDoesServiceExists(serviceName);
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        LogHandler.WriteLogMessage(LbLogFilePath.Content.ToString(), "ServiceStatus: " + service.Status);
                    }
                    LbDownloadedProcessStatus.Content = "Changes saved, service status: " + service.Status;
                    LvDownLoadedConnectionSettings.ItemsSource = "";
                    LvDownloadedConfigSettings.ItemsSource = "";
                    LbExistingAppSettingsFilePath.Content = "";
                    BnSaveDownloadedAppSettingsChanges.IsEnabled = false;
                    scope.Complete();
                }
                catch (Exception error)
                {
                    LbDownloadedProcessStatus.Content = error.Message;
                    string errorMessage = LogHandler.CreateErrorMessage(error);
                    LogHandler.WriteLogMessage(LbLogFilePath.Content.ToString(), errorMessage);
                }
            }
        }


        private void BnCloseDownload_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        /// <summary>
        /// Controls does current service exists
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <returns>Returns true, if service exists</returns>
        public static bool DoesServiceExist(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach(ServiceController service in services)
            {
                if (service.ServiceName == serviceName)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Controls that service was downloaded successfully
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <returns>Returns true, if service was downloaded successfully</returns>
        public static bool FindDoesServiceExists(string serviceName)
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                if (service.ServiceName == serviceName)
                {
                    return true;
                }
            }
            var timeout = new TimeSpan(0, 0, 5);
            FindDoesServiceExists(serviceName);
            return false;
        }
        /// <summary>
        /// Downloads new service
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="exePath">Exe -file location</param>
        public static void CreateService(string serviceName, string exePath)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(); 
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = "C:\\Windows\\System32\\sc.exe";

            startInfo.Arguments = "create "+serviceName+ " binPath= "+exePath;
            process.StartInfo = startInfo;
            process.Start();

        }
    }
}
