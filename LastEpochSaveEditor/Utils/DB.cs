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
	internal sealed class DB
    {
        private const string URL = "https://assets-ng.maxroll.gg/leplanner/game/data.json";
		private const string FILE_PATH = "data.json";

		private static readonly Lazy<DB> _instance = new Lazy<DB>(() => new DB());
		public static DB Instance => _instance.Value;

		private Database Database { get; set; }

		private static async Task LoadImpl()
        {
			string response;
			using (var client = new HttpClient())
			{
				using (var request = await client.GetAsync(URL))
					response = await request.Content.ReadAsStringAsync();
			}

			Instance.Database = JsonConvert.DeserializeObject<Database>(response);
		}

		internal static async Task Load()
		{
			await LoadImpl();
			if (!File.Exists(FILE_PATH))
				await File.WriteAllTextAsync(FILE_PATH, JsonConvert.SerializeObject(Instance.Database, Formatting.Indented));
		}

		internal static async Task Reload()
		{
			if (File.Exists(FILE_PATH))
				File.Delete(FILE_PATH);

			await Load();
		}

        private DB() { }

		private IEnumerable<SubItem> Get(string type, string subType, int subTypeId)
		{
			var categories = Instance.Database?.ItemCategories?.FirstOrDefault(x => string.Equals(x.Name, type, StringComparison.OrdinalIgnoreCase))?.Categories;
			if (categories == null)
				throw new CategoryNotFoundException(type, subType, subTypeId);

			var subCategories = categories.FirstOrDefault(x => string.Equals(x.Name, subType, StringComparison.OrdinalIgnoreCase))?.Entries;
			if (subCategories == null)
				throw new CategoryNotFoundException(type, subType, subTypeId);

			var itemTypes = Instance!.Database!.ItemTypes.Where(x => subCategories.Contains(x.BaseTypeID));
			if (!itemTypes.Any())
				throw new CategoryNotFoundException(type, subType, subTypeId);

			var subItems = itemTypes.FirstOrDefault(x => x.BaseTypeID == subTypeId)!.SubItems.Where(x => x.CannotDrop == false);
			if (!subItems.Any())
				throw new CategoryNotFoundException(type, subType, subTypeId);

			return subItems;
		}

		public IEnumerable<SubItem> GetHelmets() => Get("Armour", "Armour", 0);

		public IEnumerable<SubItem> GetAmulets() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetWeapons() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetArmors() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetOffHands() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetRings() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetBelts() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetGloves() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetBoots() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetRelics() => Get(string.Empty, string.Empty, -1);

		public IEnumerable<SubItem> GetIdols() => Get(string.Empty, string.Empty, -1);

	}
}

#pragma warning restore CS8601