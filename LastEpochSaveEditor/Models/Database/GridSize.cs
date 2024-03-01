namespace LastEpochSaveEditor.Models.Database;

public class GridSize
{
	[JsonProperty("x")]
	public int X { get; set; }

	[JsonProperty("y")]
	public int Y { get; set; }
}