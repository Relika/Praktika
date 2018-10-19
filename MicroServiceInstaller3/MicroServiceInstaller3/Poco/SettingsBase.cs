using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MicroServiceInstaller3.Poco
{
    public class SettingsBase
    {
        public bool RbNewValue { get; set; }
        public bool RbExistingValue { get; set; }
        public Visibility RbNewValueVisibility { get; set; }
        public Visibility RbExistingValueVisibility { get; set; }


    }
}
