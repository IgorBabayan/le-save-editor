namespace LastEpochSaveEditor.Models;

public class ItemTypeCategory
{
	public string Name { get; set; }
	public IEnumerable<string> SubCategories { get; set; }
}
