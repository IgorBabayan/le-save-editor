namespace LastEpochSaveEditor.UI.Models.CharacterModel;

public class Option
{
	[JsonProperty("zoneIndex")]
	public int ZoneIndex { get; set; }

	[JsonProperty("modIndex")]
	public int ModIndex { get; set; }

	[JsonProperty("duration")]
	public int Duration { get; set; }

	[JsonProperty("effectModifier")]
	public double EffectModifier { get; set; }

	[JsonProperty("questEcho")]
	public bool QuestEcho { get; set; }
}