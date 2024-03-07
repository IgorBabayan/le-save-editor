namespace LastEpochSaveEditor.Models.CharacterModel;

public class TimelineDifficultyUnlock
{
	[JsonProperty("timelineID")]
	public int TimelineID { get; set; }

	[JsonProperty("progress")]
	public List<int>? Progress { get; set; }
}