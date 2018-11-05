using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MicroServiceInstaller3.Poco
{
    public class ConnectionStrings: SettingsBase
    {
        public string Name { get; set; }
        public string NewConnectionString { get; set; }
        public string ExistingConnectionString { get; set; }
        public string ProviderName { get; set; }
    

        //private Thickness tbNewCSBorder;
        //public Thickness TbNewCSBorder
        //{
        //    get { return tbNewCSBorder; }
        //    set
        //    {
        //        tbNewCSBorder = value;
        //        OnPropertyChanged("TbNewCSBorder");
        //    }

        //}
        ////public Thickness TbExistigValueBorder { get; set; }
        //private Thickness tbexistigCSborder;
        //public Thickness TbExistigCSBorder
        //{
        //    get { return tbexistigCSborder; }
        //    set
        //    {
        //        tbexistigCSborder = value;
        //        OnPropertyChanged("tbexistigCSborder");
        //    }
        //}
        //public event PropertyChangedEventHandler PropertyChanged;



        //// Create the OnPropertyChanged method to raise the event
        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}
    }
}
