using Newtonsoft.Json;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Database
{
	internal class Unique
    {
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("displayName")]
		public string DisplayName { get; set; }

		[JsonProperty("uniqueID")]
		public int UniqueID { get; set; }

		[JsonProperty("isSetItem")]
		public bool IsSetItem { get; set; }

		[JsonProperty("setID")]
		public int SetID { get; set; }

		[JsonProperty("overrideLevelRequirement")]
		public bool OverrideLevelRequirement { get; set; }

		[JsonProperty("levelRequirement")]
		public int LevelRequirement { get; set; }

		[JsonProperty("legendaryType")]
		public int LegendaryType { get; set; }

		[JsonProperty("overrideEffectiveLevelForLegendaryPotential")]
		public bool OverrideEffectiveLevelForLegendaryPotential { get; set; }

		[JsonProperty("effectiveLevelForLegendaryPotential")]
		public int EffectiveLevelForLegendaryPotential { get; set; }

		[JsonProperty("canDropRandomly")]
		public bool CanDropRandomly { get; set; }

		[JsonProperty("rerollChance")]
		public double RerollChance { get; set; }

		[JsonProperty("abilityTooltipToShowInAltTooltip")]
		public int AbilityTooltipToShowInAltTooltip { get; set; }

		[JsonProperty("itemModelType")]
		public int ItemModelType { get; set; }

		[JsonProperty("subTypeForIM")]
		public int SubTypeForIM { get; set; }

		[JsonProperty("baseType")]
		public int BaseType { get; set; }

		[JsonProperty("subTypes")]
		public List<int> SubTypes { get; set; }

		[JsonProperty("mods")]
		public List<Mod> Mods { get; set; }

		[JsonProperty("tooltipDescriptions")]
		public List<TooltipDescription> TooltipDescriptions { get; set; }

		[JsonProperty("loreText")]
		public string LoreText { get; set; }

		[JsonProperty("tooltipEntries")]
		public List<TooltipEntry> TooltipEntries { get; set; }

		[JsonProperty("oldSubTypeID")]
		public int OldSubTypeID { get; set; }

		[JsonProperty("oldUniqueID")]
		public int OldUniqueID { get; set; }
	}
}
