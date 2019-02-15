using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using CommonLibary.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceInstallClient;


namespace MicroServiceInstaller3.Tests
{
    [TestClass]
    public class ServicesHandlerTests
    {


        [TestMethod]
        public void AddResources()
        {
            //string finalLocation = "c:/Downloads/Debug.zip";
            //string service = "ServiceZip";
            //MainWindow.Resources.Add(service, finalLocation);
            //Uri uri = new Uri("Resources/final.zip", UriKind.Relative);
            //StreamResourceInfo info = Application.GetContentStream(uri);
            //System.Windows.Markup.XamlReader reader = new System.Windows.Markup.XamlReader();
            //ResourceDictionary myResourceDictionary =
            //                               (ResourceDictionary)reader.LoadAsync(info.Stream);
            //Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
        }

        [TestMethod]
        public void LogMessageTest()
        {
            string logFilePath = FShandler.CreateLogFile(@"C:\testservice\");
            LogHandler.WriteLogMessage(logFilePath, "serviceName:fdh ");
        }

        [TestMethod]
        public void DoesServiceExists()
        {
            string serviceName = "TestService";

            Assert.IsTrue(ServiceInstallClient.MainWindow.DoesServiceExist(serviceName));
        }
            
    }
}
