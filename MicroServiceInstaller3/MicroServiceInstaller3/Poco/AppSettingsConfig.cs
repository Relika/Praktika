﻿using System.ComponentModel;
using System.Windows;

namespace MicroServiceInstaller3.Poco
{
    public class AppSettingsConfig: SettingsBase
    {
        public string Key { get; set; }
        public string NewValue { get; set; }
        public string ExistingValue { get; set; }




    }
}
