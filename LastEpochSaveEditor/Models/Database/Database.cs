namespace LastEpochSaveEditor.Models.Database;

public class Database
{
	[JsonProperty("itemTypes")]
	public List<ItemType> ItemTypes { get; set; }

	[JsonProperty("affixes")]
	public List<Affix> Affixes { get; set; }

	[JsonProperty("uniques")]
	public List<Unique> Uniques { get; set; }
}