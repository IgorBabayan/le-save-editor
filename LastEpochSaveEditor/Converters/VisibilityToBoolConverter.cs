using System.Windows.Data;
using System;
using System.Windows;
using System.Globalization;

namespace LastEpochSaveEditor.Converters
{
	[ValueConversion(typeof(Visibility), typeof(bool))]
	internal class VisibilityToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var isVisible = ((Visibility)value == Visibility.Visible);
			if (IsVisibilityInverted(parameter))
				isVisible = !isVisible;

			return isVisible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var isVisible = (bool)value;
			if (IsVisibilityInverted(parameter))
				isVisible = !isVisible;

			return (isVisible ? Visibility.Visible : Visibility.Collapsed);
		}

		private static bool IsVisibilityInverted(object parameter) => (GetVisibilityMode(parameter) == Visibility.Collapsed);

		private static Visibility GetVisibilityMode(object parameter)
		{
			var mode = Visibility.Visible;
			if (parameter != null)
			{
				if (parameter is Visibility)
					mode = (Visibility)parameter;
				else
				{
					try
					{
						mode = (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString(), true);
					}
					catch (FormatException error)
					{
						throw new FormatException("Invalid Visibility specified as the ConverterParameter.  Use Visible or Collapsed.", error);
					}
				}
			}

			return mode;
		}
	}
}
