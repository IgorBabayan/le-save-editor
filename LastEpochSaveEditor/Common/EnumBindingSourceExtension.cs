namespace LastEpochSaveEditor.Common;

public class EnumBindingSourceExtension : MarkupExtension
{
	private readonly Type _enumType;
	
	public EnumBindingSourceExtension(Type enumType) => _enumType = enumType;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		var fields = _enumType.GetFields();
		var descriptions = new List<ItemEnum>();
		foreach (var field in fields)
		{
			var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
			var iconAttribute = (IconAttribute)Attribute.GetCustomAttribute(field, typeof(IconAttribute));
			if (descriptionAttribute != null && iconAttribute != null)
				descriptions.Add(new(descriptionAttribute.Description, iconAttribute.Path));
		}
		
		return descriptions;
	}
}

public record ItemEnum(string Description, string Path);