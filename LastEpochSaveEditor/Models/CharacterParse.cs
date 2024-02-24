namespace LastEpochSaveEditor.Models;

public partial class Character
{
	[JsonIgnore]
	public ClassInfo ClassInfo => ClassInfo.Parse(CharacterClass, ChosenMastery);

	[JsonIgnore]
	public string Challenge
	{
		get
		{
			var builder = new StringBuilder();
			if (SoloChallenge)
				builder.Append("Solo");

			if (Hardcore)
				builder.Append("HC");
			else
				builder.Append("SC");

			return builder.ToString();
		}
	}

	[JsonIgnore]
	public ICharacterInventory CharacterInventory { get; set; }

	public void ParseSavedData()
	{
		ParseCharacterInventory();
		//ParseCharacterStash();
		//ParseBlessings();
		//ParseIdols();
	}

	private void ParseCharacterInventory()
	{
		var data = SavedItems.Where(x => ItemDataParser.CharacterInventoryIds.Contains(x.ContainerID)).ToDictionary(x => x.ContainerID, x => x.Data);
		CharacterInventory = App.GetService<ICharacterInventory>();
		CharacterInventory.Parse(data);
	}
}