namespace LastEpochSaveEditor.Converters;

[ValueConversion(typeof(QualityType), typeof(string))]
internal class QualityToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var quality = (QualityType)value;
		return quality.ToString();
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
