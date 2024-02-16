using LastEpochSaveEditor.Models.Database;
using LastEpochSaveEditor.Utils.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#pragma warning disable CS8601

namespace LastEpochSaveEditor.Utils
{
	internal sealed class DB : IDB
	{
		private const string URL = "https://assets-ng.maxroll.gg/leplanner/game/data.json";
		private const string FILE_PATH = "data.json";

		private Database Database { get; set; }

		private async Task LoadImpl()
		{
			string response;
			using (var client = new HttpClient())
			{
				using (var request = await client.GetAsync(URL))
					response = await request.Content.ReadAsStringAsync();
			}

			Database = JsonConvert.DeserializeObject<Database>(response);
			foreach (var itemType in Database!.ItemTypes)
				itemType.SubItems = itemType.SubItems.Where(x => x.CannotDrop == false).ToList();
		}

		public async Task Load()
		{
			await LoadImpl();
			if (!File.Exists(FILE_PATH))
				await File.WriteAllTextAsync(FILE_PATH, JsonConvert.SerializeObject(Database, Formatting.Indented));
		}

		public async Task Reload()
		{
			if (File.Exists(FILE_PATH))
				File.Delete(FILE_PATH);

			await Load();
		}

		private IEnumerable<ItemType> Get(string type, string subType)
		{
			var categories = Database?.ItemCategories?.FirstOrDefault(x => string.Equals(x.Name, type, StringComparison.OrdinalIgnoreCase))?.Categories;
			if (categories == null)
				throw new CategoryNotFoundException(type, subType);

			var subCategories = categories.FirstOrDefault(x => string.Equals(x.Name, subType, StringComparison.OrdinalIgnoreCase))?.Entries;
			if (subCategories == null)
				throw new CategoryNotFoundException(type, subType);

			var itemTypes = Database!.ItemTypes.Where(x => subCategories.Contains(x.BaseTypeID));
			if (!itemTypes.Any())
				throw new CategoryNotFoundException(type, subType);

			return itemTypes;
		}

		private ItemType Get(string type, string subType, int subTypeId)
		{
			var itemTypes = Get(type, subType);
			var subItem = itemTypes.FirstOrDefault(x => x.BaseTypeID == subTypeId);
			if (subItem == null)
				throw new CategoryNotFoundException(type, subType, subTypeId);

			return subItem;
		}

		public IEnumerable<ItemType> GetWeapons() => Get1HWeapons().Union(Get2HWeapons());

		public IEnumerable<ItemType> Get1HWeapons() => Get("Weapons", "One Handed Weapons");

		public IEnumerable<ItemType> Get2HWeapons() => Get("Weapons", "Two Handed Weapons");

		public IEnumerable<ItemType> GetOffHands() => Get("Off-Hand", "Off-Hand");

		public IEnumerable<ItemType> GetIdols() => Get("Misc", "Idols");

		public ItemType GetHelmets() => Get("Armour", "Armour", 0);

		public ItemType GetAmulets() => Get("Misc", "Accessories", 20);

		public ItemType GetArmors() => Get("Armour", "Armour", 1);

		public ItemType GetRings() => Get("Misc", "Accessories", 21);

		public ItemType GetBelts() => Get("Armour", "Armour", 2);

		public ItemType GetGloves() => Get("Armour", "Armour", 4);

		public ItemType GetBoots() => Get("Armour", "Armour", 3);

		public ItemType GetRelics() => Get("Misc", "Accessories", 22);
	}
}

#pragma warning restore CS8601