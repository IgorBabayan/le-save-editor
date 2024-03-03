namespace LastEpochSaveEditor.Models.CharacterModel;

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

			builder.Append(Hardcore ? "HC" : "SC");
			return builder.ToString();
		}
	}

	[JsonIgnore]
	public ICharacterInventory Inventory { get; set; }

	public async Task ParseSavedData()
	{
		await ParseCharacterInventory();
		ParseCharacterStash();
		ParseBlessings();
		ParseIdols();
	}

	private void ParseIdols()
	{
		
	}

	private void ParseBlessings()
	{
		
	}

	private void ParseCharacterStash()
	{
		
	}

	private async Task ParseCharacterInventory()
	{
		var data = SavedItems.Where(x => ItemDataParser.CharacterInventoryIds.Contains(x.ContainerID)).ToDictionary(x => x.ContainerID, x => x.Data);
		Inventory = App.GetService<ICharacterInventory>();
		await Inventory.Parse(data);
	}
}