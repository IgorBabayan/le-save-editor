namespace LastEpochSaveEditor.Models.Database;

public class Affix
{
	[JsonProperty("affixName")]
	public string? AffixName { get; set; }

	[JsonProperty("affixDisplayName")]
	public string? AffixDisplayName { get; set; }

	[JsonProperty("affixLootFilterOverrideName")]
	public string? AffixLootFilterOverrideName { get; set; }

	[JsonProperty("titleType")]
	public int TitleType { get; set; }

	[JsonProperty("affixTitle")]
	public string? AffixTitle { get; set; }

	[JsonProperty("affixMorphology")]
	public int AffixMorphology { get; set; }

	[JsonProperty("affixId")]
	public int AffixId { get; set; }

	[JsonProperty("levelRequirement")]
	public int LevelRequirement { get; set; }

	[JsonProperty("rollsOn")]
	public int RollsOn { get; set; }

	[JsonProperty("specialAffixType")]
	public int SpecialAffixType { get; set; }

	[JsonProperty("classSpecificity")]
	public int ClassSpecificity { get; set; }

	[JsonProperty("type")]
	public int Type { get; set; }

	[JsonProperty("standardAffixEffectModifier")]
	public double StandardAffixEffectModifier { get; set; }

	[JsonProperty("rerollChance")]
	public double RerollChance { get; set; }

	[JsonProperty("weaponEffect")]
	public int WeaponEffect { get; set; }

	[JsonProperty("group")]
	public int Group { get; set; }

	[JsonProperty("shardHueShift")]
	public double ShardHueShift { get; set; }

	[JsonProperty("shardSaturationModifier")]
	public double ShardSaturationModifier { get; set; }

	[JsonProperty("canRollOn")]
	public List<int>? CanRollOn { get; set; }

	[JsonProperty("specificRerollChances")]
	public List<SpecificRerollChance>? SpecificRerollChances { get; set; }

	[JsonProperty("convertOnIncompatibleItemType")]
	public bool ConvertOnIncompatibleItemType { get; set; }

	[JsonProperty("affixIDToConvertTo")]
	public int AffixIDToConvertTo { get; set; }

	[JsonProperty("tiers")]
	public List<Tier>? Tiers { get; set; }

	[JsonProperty("t6Compatibility")]
	public int T6Compatibility { get; set; }

	[JsonProperty("maximumAffixEffectModifierForT6")]
	public int MaximumAffixEffectModifierForT6 { get; set; }

	[JsonProperty("displayCategory")]
	public int DisplayCategory { get; set; }

	[JsonProperty("property")]
	public int Property { get; set; }

	[JsonProperty("specialTag")]
	public int SpecialTag { get; set; }

	[JsonProperty("tags")]
	public int Tags { get; set; }

	[JsonProperty("modifierType")]
	public int ModifierType { get; set; }

	[JsonProperty("setProperty")]
	public bool SetProperty { get; set; }

	[JsonProperty("useGeneratedNameForDisplayName")]
	public bool UseGeneratedNameForDisplayName { get; set; }
}