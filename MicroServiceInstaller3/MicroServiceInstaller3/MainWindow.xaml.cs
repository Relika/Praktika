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
using MicroServiceInstaller3.Handlers;
using System.Threading;
using System.Threading.Tasks;

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
            string serviceZipDirectory = ConfigurationManager.AppSettings["serviceZipDirectory"];
            LbServiceZipFolder.Content = FShandler.MakeDirectorytoTemp(serviceZipDirectory);
            string installpDirectory = ConfigurationManager.AppSettings["installDirectory"];
            LbInstallFolder.Content = FShandler.MakeDirectorytoTemp(installpDirectory);
            string programLocation = FShandler.GetProgramLocation();
            LbLogFilePath.Content = FShandler.CreateLogFile(programLocation);
        }
        string randomFileName = "";


        private void BSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            LbStatus.Content = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog(); // avab failide valimise akna
            // m''rab parameetrid
            folderBrowserDialog1.Description = "Select directory";
            folderBrowserDialog1.ShowNewFolderButton = false;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            // kui kasutaja valib kausta ja vajutab OK
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath =FShandler.ChooseFolder(folderBrowserDialog1, selectedFolderLabel: LbSelectedFolder, savebutton: BnZip);
                string workFilesFolderPath = LbworkFilesFolder.Content.ToString();
                // kopeerib valitud kasutas olevad failid nn t;;folderisse
                FShandler.DirectoryCopy(selectedPath, workFilesFolderPath, copySubDirs: true);
                // loob faililisti
                IEnumerable<string> unFilteredFileList = CreateUnFilteredFileList(workFilesFolderPath);
                // filtreerib faililisti
                FilterFileList(unFilteredFileList);
                // loob metaandmetega faili
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
                    bool endsIn = (file.EndsWith(".pdb") || file.Contains("/deps/") || file.Contains(".vshost.")); // kui faili asukohanimetus sisaldab j'rgmisi v''rtusi
                    if (endsIn)
                    {
                        File.Delete(file); // kustutab faili
                    }
                    else
                    {
                        ListFiles.Items.Add($"{file}");  // lisab faili listi              
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
            ObservableCollection<AppSettingsConfig> modifiedAppSettingsCollection = LvUploadedConfigSettings.ItemsSource as ObservableCollection<AppSettingsConfig>;
            Dictionary<string, AppSettingsConfig> appSettingsDictionary = ConfFileHandler.CreateAppSettingsDicitionary( modifiedAppSettingsCollection);
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
            try{ConfFileHandler.WriteConnectionStringstoConFile(selectedPath, connectionStringsDic: connectionStringsDictionary);}
            catch (Exception error) { LbProcessStatus.Content = error.Message;}
        }

        private void BnZip_Click(object sender, RoutedEventArgs e)
        {
            string initialsFilesFolder = LbworkFilesFolder.Content.ToString();
            string zipFileFolder = LbZipFilesFolder.Content.ToString();
            randomFileName = Guid.NewGuid().ToString();
            string zipFile = System.IO.Path.Combine(zipFileFolder, randomFileName + ".zip"); // M''rab zip faili asukoha aadressi

            BnConfig.IsEnabled = false;
            BnSaveChanges.IsEnabled = false;
            using (var scope = new TransactionScope())
            {
                ZipFile.CreateFromDirectory(initialsFilesFolder, zipFile); // loob zip faili
                bool existItem = false;
                foreach (var value in ListZipFiles.Items) //kontrollib, kas failiasukohanimetused vastavad j'rgmistele tingimustele
                {
                    string ZipFilePath = value.ToString();
                    existItem = (ZipFilePath.Equals(zipFile)); // kui on olemas sama asukohanimetusega fail
                    if (existItem)break; // l6peta tegevus
                }
                if (!existItem)ListZipFiles.Items.Add($"{zipFile}"); // kui faili ei ole, lisa see
                // eemaldab ebavajaliku info
                ListFiles.Items.Clear(); 
                LbSelectedFolder.Content = ""; 
                LbappSettingsPath.Content = "";
                LvUploadedConfigSettings.ItemsSource = "";
                LvUploadedConnectionSettings.ItemsSource = "";
                LbProcessStatus.Content = "";
                if (ListZipFiles.HasItems) BnFinishandZip.IsEnabled = true; // kui zipfailide listis on faile muudab finalzip nupu aktiivseks
                scope.Complete();
            }
            BnZip.IsEnabled = false;
        }

        private  void BnFinishandZip_Click(object sender, RoutedEventArgs e)
        {
            try { 
                string zipLocation = LbZipFilesFolder.Content.ToString(); // defineerib kausta, kus on k6ik zipitud failid
                // Create finalZip
                string serviceZipDirectory =  LbServiceZipFolder.Content.ToString(); // defineerib kausta, kus on k6ik teenuse failid, mis zipitakse
                string finalZipFileName = "final.zip"; // defineerib l6ppzipfaili nime
                string finalZipFilePath = System.IO.Path.Combine(serviceZipDirectory, finalZipFileName); // defineerib l6ppzipfaili aadressi
                // kui l6ppzipfail eksisteerib, siis kutsutab selle
                if (File.Exists(finalZipFilePath))File.Delete(finalZipFilePath);
                // zipi k6ik zipfailid l6ppzipfailiks
                ZipFile.CreateFromDirectory(zipLocation, finalZipFilePath);
                LbStatus.Content = "Zip file is created successfully: " + serviceZipDirectory; // teade kasutajale l6ppzipfaili loomisest;
                
                // defineerib kausta, kuhu kopeeritakse k6ik teenuse k'ivitamiseks vajalikud failid
                string installServiceDirectory = LbInstallFolder.Content.ToString(); 
                string confFilePath = Path.Combine(installServiceDirectory, "config.txt"); // defineerib conffaili aadressi
                string sevenZipFilePath = Path.Combine(installServiceDirectory, "7zS.sfx");// defineerib 7zs faili aadressi
                // kui kausta ei ekstisteeri, loob uue kausta
                if (!Directory.Exists(installServiceDirectory)) Directory.CreateDirectory(installServiceDirectory);
                // kopeerib teenuse failid ressurssidest teenuse failide kausta ja teenuse k'ivitamiseks vajalikud failid eraldi kausta 
                ServiceFileHandler.CopyResources(installServiceDirectory, serviceZipDirectory);
                // zipib teenusefailid teenuse k'ivitamiseks vajalike failide kausta
                string serviceFilePath = ServiceFileHandler.CreateServiceZip(serviceZipDirectory, installServiceDirectory);
                // salvestab teenuse failid ja teenuse automaatseks k'ivitamiseks vajalikud failid yhte installifaili
                string logFilePath = LbLogFilePath.Content.ToString();

                var progressHandler = new Progress<string>(value =>
                {
                    LbStatus.Content = value;
                });
                var progress = progressHandler as IProgress<string>;
                try
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        
                        string installerexePath = ServiceFileHandler.CreateInstallExe(confFilePath, serviceFilePath, sevenZipFilePath, installServiceDirectory);

                        for (int i = 0; i != 100; ++i)
                        {
                            if (progress != null) progress.Report(i + "%");
                           //Task.Delay(100);
                            Thread.Sleep(100);
                            if (File.Exists(installerexePath))
                            {
                                Dispatcher.Invoke(() => {LbStatus.Content = "Completed: " + installerexePath;});
                                break;
                            }
                        }
                        if (!File.Exists(installerexePath))
                        {
                            throw new Exception("Installer.exe not found.");
                        }                       
                    }, TaskCreationOptions.LongRunning);

                    Task task2 = task.ContinueWith((p) =>
                   {
                       if (p.Exception != null) {
                           Dispatcher.Invoke(() => { LbStatus.Content = p.Exception;});
                       }
                           //ErrorHandler.WriteLogMessage(LbLogFilePath.Content.ToString(), "installServiceDirectory: " + installServiceDirectory);
                       string desktopFilePath = ServiceFileHandler.CopyExeFile(installServiceDirectory, logFilePath); // kopeerib exe faili
                       if (desktopFilePath == "") {
                           throw new Exception("Installer.exe wasn't copied, find file from: " + installServiceDirectory);
                       }
                       else {
                           Dispatcher.Invoke(() => {LbStatus.Content = ("Installer.exe is copied successfully: " + desktopFilePath);});
                       }
                   });

                    Task task3 = task2.ContinueWith((p) =>
                    {
                        if (p.Exception != null){
                            Dispatcher.Invoke(() => { LbStatus.Content = p.Exception; });
                        } else {
                            Dispatcher.Invoke(() => { BnFinishandZip.IsEnabled = false; ListZipFiles.Items.Clear(); });
                        }
                    });
                }
                catch (Exception ex)
                {
                    LbStatus.Content = ex.GetType().Name + ": " + ex.Message;
                }
            }
            catch (Exception error)
            {
                LogHandler.WriteLogMessage(LbLogFilePath.Content.ToString(), "error: " + error);
            }
        }

        private void BnCloseUpload_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

    }
}
