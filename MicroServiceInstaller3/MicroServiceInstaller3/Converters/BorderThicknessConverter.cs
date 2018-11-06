using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace MicroServiceInstaller3.Converters
{
    public class BorderThicknessConverter : System.Windows.Markup.MarkupExtension, IValueConverter
    {
        public object Convert(Boolean value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == true) {
                return Thickness.Equals(3.0, 3.0);
            }
            else {
                return Thickness.Equals(1.0, 1.0);
            }//(value as ToggleButton)
            //    ? BorderThickness. : Visibility.Visible;

            //item.TbExistigValueBorder = new Thickness(3.0);
            //item.TbNewValueBorder = new Thickness(1.0);

            // return string.IsNullOrEmpty(value as string)
            // ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(Boolean value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    throw new NotImplementedException();
        //}

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
