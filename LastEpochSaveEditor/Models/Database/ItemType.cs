namespace LastEpochSaveEditor.Models.Database;

internal class ItemType
{
	[JsonProperty("BaseTypeName")]
	public string BaseTypeName { get; set; }

	[JsonProperty("displayName")]
	public string DisplayName { get; set; }

	[JsonProperty("lootFilterNameOverride")]
	public string LootFilterNameOverride { get; set; }

	[JsonProperty("baseTypeID")]
	public int BaseTypeID { get; set; }

	[JsonProperty("maximumAffixes")]
	public int MaximumAffixes { get; set; }

	[JsonProperty("maxSockets")]
	public int MaxSockets { get; set; }

	[JsonProperty("affixEffectModifier")]
	public double AffixEffectModifier { get; set; }

	[JsonProperty("gridSize")]
	public GridSize GridSize { get; set; }

	[JsonProperty("type")]
	public int Type { get; set; }

	[JsonProperty("isWeapon")]
	public bool IsWeapon { get; set; }

	[JsonProperty("minimumDropLevel")]
	public int MinimumDropLevel { get; set; }

	[JsonProperty("goldCostModifier")]
	public double GoldCostModifier { get; set; }

	[JsonProperty("addedGambleCost")]
	public int AddedGambleCost { get; set; }

	[JsonProperty("rerollChance")]
	public double RerollChance { get; set; }

	[JsonProperty("classAffinity")]
	public int ClassAffinity { get; set; }

	[JsonProperty("affinityRerollChance")]
	public int AffinityRerollChance { get; set; }

	[JsonProperty("subTypeDropUpgradeChanceMultiplier")]
	public double SubTypeDropUpgradeChanceMultiplier { get; set; }

	[JsonProperty("subTypeDropUpgradeChanceMultiplierPerLevel")]
	public double SubTypeDropUpgradeChanceMultiplierPerLevel { get; set; }

	[JsonProperty("subTypeClassSpecificity")]
	public int SubTypeClassSpecificity { get; set; }

	[JsonProperty("usePreClassIncompatibilityDropLists")]
	public bool UsePreClassIncompatibilityDropLists { get; set; }

	[JsonProperty("preClassIncompatibilityDropLists")]
	public List<PreClassIncompatibilityDropList> PreClassIncompatibilityDropLists { get; set; }

	[JsonProperty("subItems")]
	public List<SubItem> SubItems { get; set; }
}