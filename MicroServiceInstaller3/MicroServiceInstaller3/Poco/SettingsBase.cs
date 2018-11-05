using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MicroServiceInstaller3.Poco
{
    public class SettingsBase: INotifyPropertyChanged
    {
        public bool IsValueExist { get; set; } // kui on eelnevalt eksisteeriv v''rtyus olemas
        public bool IsValueNew { get; set; } // kui uus v''rtus on olemas
        public bool RbNewValue { get; set; }
        public bool RbExistingValue { get; set; }
        //public Visibility RbNewValueVisibility { get; set; }
        //public Visibility RbExistingValueVisibility { get; set; }
        //public Visibility TbValueVisibility { get; set; }
        //public Visibility TbExistingValueVisibility { get; set; }

        private Thickness tbNewValueBorder;
        public Thickness TbNewValueBorder
        {
            get { return tbNewValueBorder; }
            set
            {
                tbNewValueBorder = value;
                OnPropertyChanged("TbNewValueBorder");
            }

        }
        //public Thickness TbExistigValueBorder { get; set; }
        private Thickness tbexistigvalueborder;
        public Thickness TbExistigValueBorder
        {
            get { return tbexistigvalueborder; }
            set
            {
                tbexistigvalueborder = value;
                OnPropertyChanged("tbexistigvalueborder");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
