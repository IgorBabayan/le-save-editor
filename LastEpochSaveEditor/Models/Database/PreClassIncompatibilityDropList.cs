namespace LastEpochSaveEditor.Models.Database;

public class PreClassIncompatibilityDropList
{
	[JsonProperty("list")]
	public List<int> List { get; set; }
}