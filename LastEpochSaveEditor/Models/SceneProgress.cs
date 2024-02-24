namespace LastEpochSaveEditor.Models;

public class SceneProgress
{
	[JsonProperty("scene")]
	public string Scene { get; set; }

	[JsonProperty("savedProgress")]
	public object SavedProgress { get; set; }

	[JsonProperty("version")]
	public int Version { get; set; }
}