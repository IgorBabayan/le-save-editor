namespace LastEpochSaveEditor.Utils;

internal static class SaveFileLoader
{
	public static IEnumerable<CharacterInfo> LoadDebug()
	{
		const string FILE = "C:\\Users\\ihorb\\AppData\\LocalLow\\Eleventh Hour Games\\Last Epoch\\Saves\\1CHARACTERSLOT_BETA_2";
		var result = new List<CharacterInfo>(1);
		var content = File.ReadAllText(FILE).Remove(0, Consts.EPOCH.Length);
		var character = JsonConvert.DeserializeObject<Character>(content)!;
		character.ParseSavedData();
		result.Add(new CharacterInfo
		{
			Path = FILE,
			Character = character
		});

		return result;
	}

	public static IEnumerable<CharacterInfo> Load(string specialCharacterSlot = null)
	{
		var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var path = Path.Combine(userFolder, Consts.ADD_DATA, Consts.LOCAL_LOW, Consts.ELEVENTH_HOUR_GAMES, Consts.LAST_EPOCH, Consts.SAVES);

		if (!Directory.Exists(path))
			throw new DirectoryNotFoundException();

		var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
			.Where(x => !x.EndsWith(Consts.BAK, StringComparison.OrdinalIgnoreCase) && !x.Contains(Consts.TEMP) && x.Contains(Consts.CHARACTER_SLOT));

		if (!string.IsNullOrEmpty(specialCharacterSlot))
			files = files.Where(x => string.Equals(specialCharacterSlot, Path.GetFileNameWithoutExtension(x), StringComparison.OrdinalIgnoreCase));

		var result = new List<CharacterInfo>(files.Count());
		string content;
		foreach ( var file in files)
		{
			content = File.ReadAllText(file).Remove(0, Consts.EPOCH.Length);
			var character = JsonConvert.DeserializeObject<Character>(content)!;
			character.ParseSavedData();

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