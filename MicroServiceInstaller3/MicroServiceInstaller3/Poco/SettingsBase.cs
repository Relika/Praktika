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
        public bool IsValueExist { get; set; } // kui on eelnevalt eksisteeriv v''rtyus olemas
        public bool IsValueNew { get; set; } // kui uus v''rtus on olemas
        public bool RbNewValue { get; set; }
        public bool RbExistingValue { get; set; }
        public Visibility RbNewValueVisibility { get; set; }
        public Visibility RbExistingValueVisibility { get; set; }
        public Visibility TbValueVisibility { get; set; }
        public Visibility TbExistingValueVisibility { get; set; }


    }
}
