namespace LastEpochSaveEditor.Models.CharacterModel;

public class SavedEchoWeb
{
	[JsonProperty("version")]
	public int Version { get; set; }

	[JsonProperty("corruption")]
	public int Corruption { get; set; }

	[JsonProperty("echoesSinceLastDeath")]
	public int EchoesSinceLastDeath { get; set; }

	[JsonProperty("gazeOfOrobyss")]
	public int GazeOfOrobyss { get; set; }

	[JsonProperty("islands")]
	public List<Island>? Islands { get; set; }
}