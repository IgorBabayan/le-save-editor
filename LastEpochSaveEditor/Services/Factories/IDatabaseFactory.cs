namespace LastEpochSaveEditor.Services.Factories;

public interface IDatabaseFactory
{
    Task<Database> Create(bool reload = false);
}

public class DatabaseFactory : IDatabaseFactory
{
    private async Task<Database> Reload()
    {
        if (File.Exists(Const.DATA_FILE_PATH))
            File.Delete(Const.DATA_FILE_PATH);
        
        var content = await FileDownloader.DownloadDatabase();
        var database = JsonConvert.DeserializeObject<Database>(content)!;
        foreach (var itemType in database!.ItemTypes)
            itemType.SubItems = itemType.SubItems.Where(x => x.CannotDrop == false).ToList();
            
        await File.WriteAllTextAsync(Const.DATA_FILE_PATH, JsonConvert.SerializeObject(database, Formatting.Indented));
        return database;
    }
    
    public async Task<Database> Create(bool reload = false)
    {
        Database database;
        if (reload || !File.Exists(Const.DATA_FILE_PATH))
        {
            database = await Reload();
            return database;
        }

        var content = await File.ReadAllTextAsync(Const.DATA_FILE_PATH);
        database = JsonConvert.DeserializeObject<Database>(content)!;
        return database;
    }
}