using Newtonsoft.Json;

namespace LastEpochSaveEditor.Models.Database
{
	internal class SpecificRerollChance
	{
		[JsonProperty("equipmentType")]
		public int EquipmentType { get; set; }

		[JsonProperty("rerollChance")]
		public double RerollChance { get; set; }
	}
}
