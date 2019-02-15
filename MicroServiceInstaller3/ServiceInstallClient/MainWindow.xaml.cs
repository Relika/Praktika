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
            //PrintResources();
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
                // Leia kaustas zip file
                string temporaryDirectory = FShandler.MakeRandomDirectorytoTemp();
                ZipFile.ExtractToDirectory(zipFile, temporaryDirectory);

                if (ConfFileHandler.FindZipFile(temporaryDirectory) == ""){// rohkem zip faile ei ole
                        IEnumerable<string> unFilteredFileList = CreateUnFilteredZipFileList(temporaryDirectory);
                    FilterZipFileList(unFilteredFileList);
                } else{
                    IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(temporaryDirectory, "*", SearchOption.AllDirectories);
                    ListUnPackedZipFiles.ItemsSource = Files;
                    foreach (object item in ListUnPackedZipFiles.ItemsSource)
                    {
                        string temporaryDirectory2 = FShandler.MakeRandomDirectorytoTemp();
                        ZipFile.ExtractToDirectory(item.ToString(), temporaryDirectory2);
                        IEnumerable<string> unFilteredFileList = CreateUnFilteredZipFileList(temporaryDirectory2);
                        FilterZipFileList(unFilteredFileList);
                    }
                }
            }
            catch (Exception error)
            {
                LbProcessStatus.Content = "No zipfile found"+error.Message;
            }
            finally
            {
                BTestSelectZipFile.IsEnabled = false;
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
                //LbTemporaryFolderZipFile.Content = selectedPath;
                // logfaili loomine
                //LbLogFilePath.Content = FShandler.CreateLogFile(selectedPath);
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
                LbLogFilePath.Content = FShandler.CreateLogFile(selectedPath);
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
            //string serviceFileName = System.IO.Path.Combine(serviceName, ".exe");
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
                        InstallService(exeFilePath);
                        service.Start();
                        //if (service.Status == ServiceControllerStatus.)
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
            return false; //return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(serviceName));
        }

        public static void InstallService(string exeFilename)
        {
            //string[] commandLineOptions = new string[1] { "/LogFile=install.log" };

            //AssemblyInstaller installer = new AssemblyInstaller(exeFilename, commandLineOptions);

            //installer.UseNewContext = true;
            //installer.Install(null);
            //installer.Commit(null);
            //public void InstallWinService(string winServicePath)
            //{
            //ServiceInstaller.
                try
                {
                    ManagedInstallerClass.InstallHelper(new string[] { exeFilename });
                }
                catch (Exception)
                {

                    throw;
                }
            //}

        }

    }
}
