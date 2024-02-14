using System.Windows.Data;
using System;
using System.Windows;
using System.Globalization;

namespace LastEpochSaveEditor.Converters
{
	[ValueConversion(typeof(Visibility), typeof(Boolean))]
	internal class VisibilityToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var result = (Visibility)value;
			return result == Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var result = (bool)value;
			return result == true ? Visibility.Visible : Visibility.Hidden;
		}
	}
}
