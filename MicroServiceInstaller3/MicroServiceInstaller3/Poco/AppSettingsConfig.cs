using System.ComponentModel;
using System.Windows;

namespace MicroServiceInstaller3.Poco
{
    public class AppSettingsConfig: SettingsBase, INotifyPropertyChanged
    {
        public string Key { get; set; }
        public string NewValue { get; set; }
        public string ExistingValue { get; set; }


        private Thickness tbNewValueBorder;
        public Thickness TbNewValueBorder {
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
