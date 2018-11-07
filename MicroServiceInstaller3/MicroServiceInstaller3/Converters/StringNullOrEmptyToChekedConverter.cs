using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace MicroServiceInstaller3.Converters
{
	class StringNullOrEmptyToChekedConverter : System.Windows.Markup.MarkupExtension, IValueConverter
	{


	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{

			//string isChecked = value as string;

			if (string.IsNullOrEmpty(value as string))
			{
				return (value as ToggleButton).IsChecked = true;
			}
			else
			{
				return ToggleButton.IsChecked = false;
			}
	}
	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return null;
	}
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return this;
	}

	}
}
