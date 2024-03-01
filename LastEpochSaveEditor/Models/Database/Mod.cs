namespace LastEpochSaveEditor.Models.Database;

public class Mod
{
	[JsonProperty("value")]
	public double Value { get; set; }

	[JsonProperty("canRoll")]
	public bool CanRoll { get; set; }

	[JsonProperty("rollID")]
	public int RollID { get; set; }

	[JsonProperty("maxValue")]
	public double MaxValue { get; set; }

	[JsonProperty("property")]
	public int Property { get; set; }

	[JsonProperty("specialTag")]
	public int SpecialTag { get; set; }

	[JsonProperty("tags")]
	public int Tags { get; set; }

	[JsonProperty("type")]
	public int Type { get; set; }

	[JsonProperty("hideInTooltip")]
	public bool HideInTooltip { get; set; }
}