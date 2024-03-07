namespace LastEpochSaveEditor.Converters;

internal class QualityToColorConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var quality = (QualityType)(value ?? QualityType.Basic);
		switch (quality)
		{
			case QualityType.Magic:
				return new SolidColorBrush(Color.FromRgb(54, 163, 226));

			case QualityType.Rare:
				return new SolidColorBrush(Color.FromRgb(227, 209, 87));

			case QualityType.Exalted:
				return new SolidColorBrush(Color.FromRgb(193, 132, 255));

			case QualityType.Unique:
				return new SolidColorBrush(Color.FromRgb(188, 92, 8));

			case QualityType.Set:
				return new SolidColorBrush(Color.FromRgb(90, 186, 100));

			case QualityType.Legendary:
				return new SolidColorBrush(Color.FromRgb(235, 15, 93));

			default:
				return new SolidColorBrush(Color.FromRgb(170, 170, 170));
		}
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
