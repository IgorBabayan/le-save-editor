namespace LastEpochSaveEditor.Models.CharacterModel;

public class SavedSkillTree
{
	[JsonProperty("treeID")]
	public string? TreeID { get; set; }

	[JsonProperty("slotNumber")]
	public int SlotNumber { get; set; }

	[JsonProperty("xp")]
	public int Xp { get; set; }

	[JsonProperty("version")]
	public int Version { get; set; }

	[JsonProperty("nodeIDs")]
	public List<int>? NodeIDs { get; set; }

	[JsonProperty("nodePoints")]
	public List<int>? NodePoints { get; set; }

	[JsonProperty("unspentPoints")]
	public int UnspentPoints { get; set; }

	[JsonProperty("nodesTaken")]
	public object? NodesTaken { get; set; }

	[JsonProperty("abilityXP")]
	public double AbilityXP { get; set; }
}