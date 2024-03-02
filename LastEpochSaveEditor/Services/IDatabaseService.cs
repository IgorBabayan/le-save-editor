namespace LastEpochSaveEditor.Services;

public interface IDatabaseSerive
{
    Task Load();
    Task Reload();
    object Get(QualityType quality, ItemInfoTypeEnum type, int id);
    IEnumerable<object> Get(QualityType quality, ItemInfoTypeEnum type);


	/*IEnumerable<Item> Get1HandWeapons() => new[] { GetOneHandAxes(), GetOneHandDaggers(), GetOneHandMaces(), GetOneHandScepters(),
        GetOneHandSwords(), GetWands() };

    IEnumerable<Item> Get2HandWeapons() => new[] { GetTwoHandAxes(), GetTwoHandMaces(), GetTwoHandSpears(), GetStaffs(),
        GetTwoHandSwords(), GetBows() };

    IEnumerable<Item> GetWeapons() => Get1HandWeapons().Union(Get2HandWeapons());

    IEnumerable<Item> GetIdols() => new[] { GetSmallIdols(), GetSmallLagonianIdols(), GetHumbleIdols(), GetStoutIdols(), GetGrandIdols(),
        GetLargeIdols(), GetOrnateIdols(), GetHugeIdols(), GetAdornedIdols() };

    IEnumerable<Item> GetAccessories() => new[] { GetAmulets(), GetRings(), GetRelics() };

    IEnumerable<Item> GetArmours() => new[] { GetHelmets(), GetBodies(), GetBelts(), GetBoots(), GetGloves() };

    IEnumerable<Item> GetOffHands() => new[] { GetQuivers(), GetShields(), GetCatalysts() };

    IEnumerable<Item> GetAll() => GetOffHands()
        .Union(GetArmours())
        .Union(GetAccessories())
        .Union(Get1HandWeapons())
        .Union(Get2HandWeapons())
        .Union(GetIdols());

    int Count()
    {
        var count = 0;
        foreach (var item in GetAll())
            count += item.Base.SubItems.Count + item.Uniques.Count() + item.Sets.Count();

        return count;
    }*/

	/*Dictionary<string, IEnumerable<Item>> GetFolderStructure() => new()
    {
        ["Off Hands"] = GetOffHands(),
        ["Armours"] = GetArmours(),
        ["Accessories"] = GetAccessories(),
        ["One hand weapons"] = Get1HandWeapons(),
        ["Two hand weapons"] = Get2HandWeapons(),
        ["Idols"] = GetIdols()
    };*/
}

public class DatabaseService : IDatabaseSerive
{
    private Database? _database;
    private readonly IRepository<SubItem> _subItemRepository;
    private readonly IRepository<Unique> _uniqueRepository;
    private readonly IRepository<Unique> _setRepository;

    private async Task LoadImpl()
    {
        var response = await FileDownloader.DownloadDatabase();

        _database = JsonConvert.DeserializeObject<Database>(response)!;
        foreach (var itemType in _database!.ItemTypes)
            itemType.SubItems = itemType.SubItems.Where(x => x.CannotDrop == false).ToList();
    }

    public async Task Load()
    {
        await LoadImpl();
        if (!File.Exists(Const.DATA_FILE_PATH))
            await File.WriteAllTextAsync(Const.DATA_FILE_PATH, JsonConvert.SerializeObject(_database, Formatting.Indented));
    }

    public async Task Reload()
    {
        if (File.Exists(Const.DATA_FILE_PATH))
            File.Delete(Const.DATA_FILE_PATH);

        await Load();
    }

    public object Get(QualityType quality, ItemInfoTypeEnum type, int id)
    {
        var items = Get(quality, type);
        if (items is IEnumerable<SubItem> subItems)
            return subItems.FirstOrDefault(x => x.SubTypeID == id) ?? throw new ArgumentException(nameof(id));

        if (items is IEnumerable<Unique> uniques)
			return uniques.FirstOrDefault(x => x.SubTypes.Contains(id)) ?? throw new ArgumentException(nameof(id));
		
        throw new ArgumentException(nameof(type));
	}

    public IEnumerable<object> Get(QualityType quality, ItemInfoTypeEnum type)
    {
		if (quality.HasFlag(QualityType.NotUniqueOrSet))
			return _subItemRepository.Get(type);

		if (quality.HasFlag(QualityType.Unique) || quality.HasFlag(QualityType.Legendary))
			return _uniqueRepository.Get(type);

		if (quality.HasFlag(QualityType.Set))
			return _setRepository.Get(type);

		throw new ArgumentException(nameof(type));
	}

	public DatabaseService(ISubItemRepositoryFactory subItemFactory, IUniqueRepositoryFactory uniqueFactory, ISetRepositoryFactory setFactory,
        Database database)
    {
        _subItemRepository = subItemFactory.Create();
        _uniqueRepository = uniqueFactory.Create();
        _setRepository = setFactory.Create();
        _database = database;
    }
}