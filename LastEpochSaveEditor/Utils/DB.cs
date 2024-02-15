using LastEpochSaveEditor.Models.Database;
using Newtonsoft.Json;
using System;
using System.IO;
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

		public Database Database { get; private set; }

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
			if (!File.Exists(FILE_PATH))
			{
				await LoadImpl();
				await File.WriteAllTextAsync(FILE_PATH, JsonConvert.SerializeObject(Instance.Database, Formatting.Indented));
			}
		}

		internal static async Task Reload()
		{
			if (File.Exists(FILE_PATH))
				File.Delete(FILE_PATH);

			await Load();
		}

        private DB() { }
    }
}

#pragma warning restore CS8601