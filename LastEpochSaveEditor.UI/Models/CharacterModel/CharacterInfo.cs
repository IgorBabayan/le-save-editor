namespace LastEpochSaveEditor.Models.CharacterModel;

public class CharacterInfo
{
	public string Path { get; set; }
	public Character Character { get; set; } = new();
}