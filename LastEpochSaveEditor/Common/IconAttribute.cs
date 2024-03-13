namespace LastEpochSaveEditor.Common;

public class IconAttribute : Attribute
{
	public string Path { get; private set; }
	
	public IconAttribute(string path) => Path = path;
}