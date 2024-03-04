namespace LastEpochSaveEditor.Services.Factories;

public interface IDatabaseFactory
{
    Task<Database> Create(bool reload = false);
}

public class DatabaseFactory : IDatabaseFactory
{
	private Database? _database;

	private async Task<Database> Reload()
    {
        if (File.Exists(DATA_FILE_PATH))
            File.Delete(DATA_FILE_PATH);
        
        var content = await FileDownloader.DownloadDatabase();
        var database = JsonConvert.DeserializeObject<Database>(content)!;
        foreach (var itemType in database!.ItemTypes)
            itemType.SubItems = itemType.SubItems.Where(x => x.CannotDrop == false).ToList();
            
        await File.WriteAllTextAsync(DATA_FILE_PATH, JsonConvert.SerializeObject(database, Formatting.Indented));
        return database;
    }
    
    public async Task<Database> Create(bool reload = false)
    {
        if (reload || !File.Exists(DATA_FILE_PATH))
        {
            _database = await Reload();
            return _database;
        }

        if (_database != null)
            return _database;

		var content = await File.ReadAllTextAsync(DATA_FILE_PATH);
		_database = JsonConvert.DeserializeObject<Database>(content)!;
        return _database;
	}
}