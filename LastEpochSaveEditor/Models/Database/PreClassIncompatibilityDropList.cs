namespace LastEpochSaveEditor.Models.Database;

internal class PreClassIncompatibilityDropList
{
	[JsonProperty("list")]
	public List<int> List { get; set; }
}