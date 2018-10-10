using System.Windows;

namespace MicroServiceInstaller3.Poco
{
    public class AppSettingsConfig
    {
        public string Key { get; set; }
        public string NewValue { get; set; }
        public string ExistingValue { get; set; }
        public bool RbNewValue { get; set; }
        public bool RbExistingValue { get; set; }
        public Visibility TbValueVisibility { get; set; }
        public Visibility TbExistingValueVisibility { get; set; }
        public Visibility RbNewValueVisibility { get; set; }
        public Visibility RbExistingValueVisibility { get; set; }
        //public Thickness TbNewValueBorder { get; set; }
        //public Thickness TbExistigValueBorder { get; set; }
    }
}
