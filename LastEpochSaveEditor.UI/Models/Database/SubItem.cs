namespace LastEpochSaveEditor.Models.Database;

public class SubItem
{
	[JsonProperty("name")]
	public string? Name { get; set; }

	[JsonProperty("displayName")]
	public string? DisplayName { get; set; }

	[JsonProperty("subTypeID")]
	public int SubTypeID { get; set; }

	[JsonProperty("levelRequirement")]
	public int LevelRequirement { get; set; }

	[JsonProperty("cannotDrop")]
	public bool CannotDrop { get; set; }

	[JsonProperty("itemTags")]
	public int ItemTags { get; set; }

	[JsonProperty("hideSubTypeInLootFilter")]
	public bool HideSubTypeInLootFilter { get; set; }

	[JsonProperty("implicits")]
	public List<Implicit>? Implicits { get; set; }

	[JsonProperty("classRequirement")]
	public int ClassRequirement { get; set; }

	[JsonProperty("subClassRequirement")]
	public int SubClassRequirement { get; set; }

	[JsonProperty("cannotBeUpgradedOnDrop")]
	public bool CannotBeUpgradedOnDrop { get; set; }

	[JsonProperty("addedWeaponRange")]
	public double AddedWeaponRange { get; set; }

	[JsonProperty("attackRate")]
	public double AttackRate { get; set; }

	[JsonProperty("IMSetTier")]
	public int IMSetTier { get; set; }

	[JsonProperty("IMSetOverrides")]
	public List<IMSetOverride>? IMSetOverrides { get; set; }

	[JsonProperty("rerollChance")]
	public double? RerollChance { get; set; }

	[JsonProperty("description")]
	public string? Description { get; set; }
}