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
using System.Windows.Resources;
using System.Windows.Documents;
using System.Resources;
using System.Collections;
using System.Linq;
using System.Reflection;

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
                string serviceZipDirectory =  LbServiceZipFolder.Content.ToString(); 
                string finalZipFileName = "final.zip";
                string finalZipFilePath = System.IO.Path.Combine(serviceZipDirectory, finalZipFileName);
                if (File.Exists(finalZipFilePath))
                {
                    File.Delete(finalZipFilePath);
                }
                ZipFile.CreateFromDirectory(zipLocation, finalZipFilePath);
                LbStatus.Content = "Zip file is created successfully: " + serviceZipDirectory; //finalExe;
                BnFinishandZip.IsEnabled = false;
                ListZipFiles.Items.Clear(); // eemaldab listis olevad valitud zip failide asukohakirjed
                scope.Complete();


                string serviceExePath = Path.Combine(serviceZipDirectory, "ServiceInstallClient.exe");
                string newtonSoftDllPath = Path.Combine(serviceZipDirectory, "Newtonsoft_Json.dll");
                Properties.Resources.ResourceManager.GetObject("ServiceInstallClient");
              
                

                string installServiceDirectory = LbInstallFolder.Content.ToString();
                string confFilePath = Path.Combine(installServiceDirectory, "config.txt");
                string sevenZipFilePath = Path.Combine(installServiceDirectory, "7zS.sfx");
                if (!Directory.Exists(installServiceDirectory)) Directory.CreateDirectory(installServiceDirectory);
                // kopeerib Client programmi failid ressurssidest
                string serviceFilePath = CreateServiceZip(serviceZipDirectory, installServiceDirectory);
                //CopyResources(confFilePath, sevenZipFilePath);// kopeerib ressursid installServiceDirectorysse
                CreateInstallExe(confFilePath, serviceFilePath, sevenZipFilePath);
                CopyExeFile(installServiceDirectory);
            }
        }

        public static string[] GetAllTxt()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            string folderName = string.Format("MicroServiceInstaller3.Properties.Resources.", executingAssembly.GetName().Name);
            return executingAssembly
                .GetManifestResourceNames()
                .Where(r => r.StartsWith(folderName) && r.EndsWith(".txt"))
                //.Select(r => r.Substring(constantResName.Length + 1))
                .ToArray();
        }
        public static void CopyResources(string installServiceDirectory, string serviceZipDirectory)
        {


            const string subfolder = "MicroServiceInstaller3.Properties.Resources.";
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var name in assembly.GetManifestResourceNames())
            {
                // Skip names outside of your desired subfolder
                //if (!name.StartsWith(subfolder))
                //{
                //    continue;
                //}
                FileStream file = assembly.GetFile("config.txt");
                FileStream[] files = assembly.GetFiles();
                foreach (var item in files)
                {

                }
                using (Stream input = assembly.GetManifestResourceStream(name))
                using (Stream output = File.Create(installServiceDirectory + name.Substring(subfolder.Length)))
                {
                    input.CopyTo(output);
                }
            }

            var ressourceList = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var item in ressourceList)
            {

                if (item.EndsWith(".exe") || item.EndsWith("dll")){
                    string filePath = Path.Combine(serviceZipDirectory, item.ToString());
                    File.WriteAllBytes(filePath, Properties.Resources.ServiceInstallClient);
                    File.WriteAllBytes(filePath, Properties.Resources.CommonLibary);

                }
                if(item.Equals("config.txt") )
                {
                    string confFilePath = Path.Combine(installServiceDirectory, "config.txt");
                    string configFileText = Properties.Resources.config;
                    File.WriteAllText(confFilePath, configFileText);
                }
                if (item.Equals("7zS.sfx"))
                {
                    string sevenZipFilePath = Path.Combine(installServiceDirectory, "7zS.sfx");
                    File.WriteAllBytes(sevenZipFilePath, Properties.Resources._7zS);
                }
            }
            var filename = ressourceList.Where(x => x.EndsWith(".exe")).FirstOrDefault();


        }



        //public static void CopyResources1(string confFilePath, string sevenZipFilPath)
        //{
            
        //    string configFileText = Properties.Resources.config;          
        //    File.WriteAllText(confFilePath, configFileText);
        //}

        public static string CreateServiceZip(string temporaryDirectory, string installServiceDirectory)
        {
            
            string serviceFileName = "Install.7z";
            string serviceFilePath = System.IO.Path.Combine(installServiceDirectory, serviceFileName);
            if (File.Exists(serviceFilePath)) File.Delete(serviceFilePath);
            SevenZip.SevenZipCompressor.SetLibraryPath(@"C:\Users\IEUser\Downloads\7z920_extra\7za.dll");
            SevenZip.SevenZipCompressor compressor = new SevenZip.SevenZipCompressor();
            compressor.CompressDirectory(temporaryDirectory, serviceFilePath);
            return serviceFilePath;
        }

        public static void CreateInstallExe(string configFileName, string serviceFilePath, string sevenZipFileName)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "Installer.exe";
            startInfo.Arguments = "/C copy /b "+ configFileName +"+"+ serviceFilePath + "+"+ sevenZipFileName;

            //process.StartInfo = startInfo;
            //process.Start();
        }

        public static void CopyExeFile(string installServiceDirectory)
        {
            IEnumerable<string> Files = Directory.EnumerateFileSystemEntries(installServiceDirectory);
            foreach (var item in Files)
            {
                if (File.Exists(item)) continue;
                bool endsIn = (item.EndsWith(".exe")); // kui faili asukohanimetus sisaldab j'rgmis v''rtusi
                if (endsIn)
                {
                    File.Copy(item, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                }

            }
        }


        private void BnCloseUpload_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

    }
}
