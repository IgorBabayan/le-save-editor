namespace LastEpochSaveEditor.Models.Database;

internal class Tier
{
	[JsonProperty("minRoll")]
	public double MinRoll { get; set; }

	[JsonProperty("maxRoll")]
	public double MaxRoll { get; set; }

	[JsonProperty("extraRolls")]
	public List<object> ExtraRolls { get; set; }
}