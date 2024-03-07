namespace LastEpochSaveEditor.Models.CharacterModel;

public class MonolithRun
{
	[JsonProperty("timelineID")]
	public int TimelineID { get; set; }

	[JsonProperty("difficultyIndex")]
	public int DifficultyIndex { get; set; }

	[JsonProperty("depth")]
	public int Depth { get; set; }

	[JsonProperty("questCompletion")]
	public int QuestCompletion { get; set; }

	[JsonProperty("questBranch")]
	public int QuestBranch { get; set; }

	[JsonProperty("bossLootDropped")]
	public bool BossLootDropped { get; set; }

	[JsonProperty("savedEchoWeb")]
	public SavedEchoWeb? SavedEchoWeb { get; set; }

	[JsonProperty("stability")]
	public int Stability { get; set; }

	[JsonProperty("previousEchoWasQuestEcho")]
	public bool PreviousEchoWasQuestEcho { get; set; }

	[JsonProperty("previousEchoHexIndex")]
	public int PreviousEchoHexIndex { get; set; }

	[JsonProperty("previousEchoIslandType")]
	public int PreviousEchoIslandType { get; set; }

	[JsonProperty("hasRerolledOptions")]
	public bool HasRerolledOptions { get; set; }

	[JsonProperty("modIndexs")]
	public List<int>? ModIndexs { get; set; }

	[JsonProperty("modRemainingDurations")]
	public List<int>? ModRemainingDurations { get; set; }

	[JsonProperty("modEffectModifiers")]
	public List<double>? ModEffectModifiers { get; set; }

	[JsonProperty("options")]
	public List<Option>? Options { get; set; }
}