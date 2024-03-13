namespace LastEpochSaveEditor.Converters;

public class WidthToX2Converter : IMultiValueConverter
{
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length == 1 && values[0] is double actualWidth)
			return actualWidth;
		
		return DependencyProperty.UnsetValue;
	}
}