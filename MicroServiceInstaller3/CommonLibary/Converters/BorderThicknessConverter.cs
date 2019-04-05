using System;
using System.Windows;
using System.Windows.Data;


namespace CommonLibary.Converters
{
    public class BorderThicknessConverter : System.Windows.Markup.MarkupExtension, IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Is checked value</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(Object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			bool isChecked = (bool)value;
			//return (isCheked == true) ? return Thickness.Equals(3.0, 3.0): Thickness.Equals(1.0, 1.0);
			if (isChecked == true)
			{
				return new Thickness(3.0);
			}
			else
			{
				return new Thickness(1.0);
			}
        }

        public object ConvertBack(Object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
