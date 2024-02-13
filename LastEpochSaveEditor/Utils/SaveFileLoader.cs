using LastEpochSaveEditor.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LastEpochSaveEditor.Utils
{
	internal static class SaveFileLoader
	{
		private const string EPOCH = "EPOCH";
		private const string ADD_DATA = "AppData";
		private const string LOCAL_LOW = "LocalLow";
		private const string ELEVENTH_HOUR_GAMES = "Eleventh Hour Games";
		private const string LAST_EPOCH = "Last Epoch";
		private const string SAVES = "Saves";
		private const string CHARACTER_SLOT = "1CHARACTERSLOT_BETA_";
		private const string BAK = ".bak";
		private const string TEMP = "_temp";

		public static IEnumerable<CharacterInfo> Load()
		{
			var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			var path = Path.Combine(userFolder, ADD_DATA, LOCAL_LOW, ELEVENTH_HOUR_GAMES, LAST_EPOCH, SAVES);

			if (!Directory.Exists(path))
				throw new DirectoryNotFoundException();

			var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
				.Where(x => !x.EndsWith(BAK, StringComparison.OrdinalIgnoreCase) && !x.Contains(TEMP) && x.Contains(CHARACTER_SLOT));
			
			var result = new List<CharacterInfo>(files.Count());
			string content;
			foreach ( var file in files)
			{
				content = File.ReadAllText(file).Remove(0, EPOCH.Length);
				result.Add(new CharacterInfo
				{
					Path = file,
					Character = JsonConvert.DeserializeObject<Character>(content)!
				});
			}
			return result;
		}

		public static void Save(IEnumerable<CharacterInfo> characters)
		{
			throw new NotImplementedException();
		}
	}
}
