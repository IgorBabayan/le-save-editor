namespace LastEpochSaveEditor.Models.Database;

public class SpecificRerollChance
{
	[JsonProperty("equipmentType")]
	public int EquipmentType { get; set; }

	[JsonProperty("rerollChance")]
	public double RerollChance { get; set; }
}