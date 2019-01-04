using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroServiceInstaller3;
using CommonLibary.Handlers;

namespace CommonLibary.Tests
{

    [TestClass]
    public class ErrorMessageTests
    {
        [TestMethod]
        public  void TestWriteMessageToErrors()
        {
            string error = "Viga";
            string path = @"C:\Users\IEUser\Downloads\Watchdog\log.txt";
            ErrorHandler.WriteErrorMessage(path, error);
            string error2 = "viga2";
            ErrorHandler.WriteErrorMessage(path, error2);
        }

        
    }
}
