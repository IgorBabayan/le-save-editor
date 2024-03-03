namespace LastEpochSaveEditor.Services;

public interface IDatabaseService
{
    Task<object> Get(QualityType quality, ItemInfoTypeEnum type, int id);
    Task<IEnumerable<object>> Get(QualityType quality, ItemInfoTypeEnum type);

    Task<object> GetWeapon(QualityType quality, int id, ItemInfoTypeEnum type);
    Task<IEnumerable<object>> GetWeapons(QualityType quality);

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

public class DatabaseService : IDatabaseService
{
    private readonly ISubItemRepositoryFactory _subItemRepositoryFactory;
    private readonly IUniqueRepositoryFactory _uniqueRepositoryFactory;
    private readonly ISetRepositoryFactory _setRepositoryFactory;
    
    public DatabaseService(ISubItemRepositoryFactory subItemFactory, IUniqueRepositoryFactory uniqueFactory,
        ISetRepositoryFactory setFactory)
    {
        _subItemRepositoryFactory = subItemFactory;
        _uniqueRepositoryFactory = uniqueFactory;
        _setRepositoryFactory = setFactory;
	}

	public async Task<object> Get(QualityType quality, ItemInfoTypeEnum type, int id)
	{
		IRepository<SubItem> subItemRepository;
		IRepository<Unique> uniqueRepository;
		switch (quality)
		{
			case QualityType.Basic:
			case QualityType.Magic:
			case QualityType.Rare:
			case QualityType.Exalted:
				subItemRepository = await _subItemRepositoryFactory.Create();
				return subItemRepository.Get(type, id);

			case QualityType.Unique:
			case QualityType.Legendary:
				uniqueRepository = await _uniqueRepositoryFactory.Create();
				return uniqueRepository.Get(type, id);

			case QualityType.Set:
				uniqueRepository = await _setRepositoryFactory.Create();
				return uniqueRepository.Get(type, id);

			default:
				throw new ArgumentException(null, nameof(type));
		}
	}

	public async Task<IEnumerable<object>> Get(QualityType quality, ItemInfoTypeEnum type)
	{
		IRepository<SubItem> subItemRepository;
		IRepository<Unique> uniqueRepository;
		switch (quality)
		{
			case QualityType.Basic:
			case QualityType.Magic:
			case QualityType.Rare:
			case QualityType.Exalted:
				subItemRepository = await _subItemRepositoryFactory.Create();
				return subItemRepository.Get(type);

			case QualityType.Unique:
			case QualityType.Legendary:
				uniqueRepository = await _uniqueRepositoryFactory.Create();
				return uniqueRepository.Get(type);

			case QualityType.Set:
				uniqueRepository = await _setRepositoryFactory.Create();
				return uniqueRepository.Get(type);

			default:
				throw new ArgumentException(null, nameof(type));
		}
	}

	public async Task<object> GetWeapon(QualityType quality, int id, ItemInfoTypeEnum type)
	{
		IRepository<SubItem> subItemRepository;
		IRepository<Unique> uniqueRepository;
		switch (quality)
		{
			case QualityType.Basic:
			case QualityType.Magic:
			case QualityType.Rare:
			case QualityType.Exalted:
				subItemRepository = await _subItemRepositoryFactory.Create();
				return subItemRepository.GetWeapon(id, type);

			case QualityType.Unique:
			case QualityType.Legendary:
				uniqueRepository = await _uniqueRepositoryFactory.Create();
				return uniqueRepository.GetWeapon(id, type);

			case QualityType.Set:
				uniqueRepository = await _setRepositoryFactory.Create();
				return uniqueRepository.GetWeapon(id, type);

			default:
				throw new ArgumentException(null, nameof(id));
		}
	}

	public async Task<IEnumerable<object>> GetWeapons(QualityType quality)
	{
		IRepository<SubItem> subItemRepository;
		IRepository<Unique> uniqueRepository;
		switch (quality)
		{
			case QualityType.Basic:
			case QualityType.Magic:
			case QualityType.Rare:
			case QualityType.Exalted:
				subItemRepository = await _subItemRepositoryFactory.Create();
				return subItemRepository.GetWeapons();

			case QualityType.Unique:
			case QualityType.Legendary:
				uniqueRepository = await _uniqueRepositoryFactory.Create();
				return uniqueRepository.GetWeapons();

			case QualityType.Set:
				uniqueRepository = await _setRepositoryFactory.Create();
				return uniqueRepository.GetWeapons();

			default:
				throw new InvalidOperationException("Could not find any weapons");
		}
	}
}
