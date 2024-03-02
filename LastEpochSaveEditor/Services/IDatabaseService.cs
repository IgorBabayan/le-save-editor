namespace LastEpochSaveEditor.Services;

public interface IDatabaseService
{
    Task<object> Get(QualityType quality, ItemInfoTypeEnum type, int id);
    Task<IEnumerable<object>> Get(QualityType quality, ItemInfoTypeEnum type);

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

    public async Task<object> Get(QualityType quality, ItemInfoTypeEnum type, int id)
    {
        if (quality.HasFlag(QualityType.NotUniqueOrSet))
        {
            var repository = await _subItemRepositoryFactory.Create();
            return repository.Get(type, id);
        }

        if (quality.HasFlag(QualityType.Unique) || quality.HasFlag(QualityType.Legendary))
        {
            var repository = await _uniqueRepositoryFactory.Create();
            return repository.Get(type, id);
        }

        if (quality.HasFlag(QualityType.Set))
        {
            var repository = await _setRepositoryFactory.Create();
            return repository.Get(type, id);
        }

        throw new ArgumentException(null, nameof(type));
    }

    public async Task<IEnumerable<object>> Get(QualityType quality, ItemInfoTypeEnum type)
    {
        if (quality.HasFlag(QualityType.NotUniqueOrSet))
        {
            var repository = await _subItemRepositoryFactory.Create();
			return repository.Get(type);
        }

        if (quality.HasFlag(QualityType.Unique) || quality.HasFlag(QualityType.Legendary))
        {
            var repository = await _uniqueRepositoryFactory.Create();
			return repository.Get(type);
        }

        if (quality.HasFlag(QualityType.Set))
        {
            var repository = await _setRepositoryFactory.Create();
			return repository.Get(type);
        }

		throw new ArgumentException(null, nameof(type));
	}
    
    public DatabaseService(ISubItemRepositoryFactory subItemFactory, IUniqueRepositoryFactory uniqueFactory,
        ISetRepositoryFactory setFactory)
    {
        _subItemRepositoryFactory = subItemFactory;
        _uniqueRepositoryFactory = uniqueFactory;
        _setRepositoryFactory = setFactory;
    }
}