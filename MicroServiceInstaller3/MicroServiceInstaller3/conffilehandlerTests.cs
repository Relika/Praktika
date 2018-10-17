using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroServiceInstaller3
{
    [TestClass]
    public class ConfFileHandlerTests
    {

        [TestMethod]
        public void FindAppSettingsFile()
        {
            string folderName = "C:/Users/User/Downloads/Test56";
            string filePath = ConfFileHandler.FindAppSettingsFile(folderName);
            Assert.IsNotNull(filePath);
        }

    }
}
