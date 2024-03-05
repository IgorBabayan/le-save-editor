namespace LastEpochSaveEditor.Services;

internal static class SaveFileLoader
{
	public static async Task<IEnumerable<CharacterInfo>> Load(string specialCharacterSlot = null)
	{
		var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var path = Path.Combine(userFolder, ADD_DATA, LOCAL_LOW, ELEVENTH_HOUR_GAMES, LAST_EPOCH, SAVES);

		if (!Directory.Exists(path))
			throw new DirectoryNotFoundException();

		var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
			.Where(x => !x.EndsWith(BAK, StringComparison.OrdinalIgnoreCase) && !x.Contains(TEMP) && x.Contains(CHARACTER_SLOT));

		if (!string.IsNullOrEmpty(specialCharacterSlot))
			files = files.Where(x => string.Equals(specialCharacterSlot, Path.GetFileNameWithoutExtension(x), StringComparison.OrdinalIgnoreCase));

		var result = new List<CharacterInfo>(files.Count());
		string content;
		foreach (var file in files)
		{
			content = File.ReadAllText(file).Remove(0, EPOCH.Length);
			var character = JsonConvert.DeserializeObject<Character>(content)!;
			await character.ParseSavedData();

			result.Add(new CharacterInfo
			{
				Path = file,
				Character = character
			});
		}
		return result;
	}

	public static void Delete(string filePath)
	{
		throw new NotImplementedException();
	}

	public static void Save(IEnumerable<CharacterInfo> characters)
	{
		throw new NotImplementedException();
	}
}
