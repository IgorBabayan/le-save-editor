using LastEpochSaveEditor.Models.Characters;
using LastEpochSaveEditor.Models.Utils;
using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace LastEpochSaveEditor.Models
{
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
		public CharacterInventory CharacterInventory { get; set; }

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
			CharacterInventory = App.GetService<CharacterInventory>();
			CharacterInventory.Parse(data);
		}
	}
}
