namespace LastEpochSaveEditor.Models;

public class TimelineDifficultyCompletion
{
	[JsonProperty("timelineID")]
	public int TimelineID { get; set; }

	[JsonProperty("progress")]
	public List<int> Progress { get; set; }
}