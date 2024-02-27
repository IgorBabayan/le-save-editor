namespace LastEpochSaveEditor.Models.CharacterModel;

public class SavedQuest
{
	[JsonProperty("questID")]
	public int QuestID { get; set; }

	[JsonProperty("questStepID")]
	public int QuestStepID { get; set; }

	[JsonProperty("state")]
	public int State { get; set; }

	[JsonProperty("questBranch")]
	public int QuestBranch { get; set; }

	[JsonProperty("completeObjectives")]
	public List<int> CompleteObjectives { get; set; }

	[JsonProperty("failedObjectives")]
	public List<int> FailedObjectives { get; set; }

	[JsonProperty("nolongerRelevantObjectives")]
	public List<object> NolongerRelevantObjectives { get; set; }

	[JsonProperty("objectiveProgress")]
	public List<object> ObjectiveProgress { get; set; }
}