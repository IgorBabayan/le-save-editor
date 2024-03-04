namespace LastEpochSaveEditor.Services;

public interface IDatabaseService
{
	Task<int> Count();

    Task<object> Get(QualityType quality, ItemInfoTypeEnum type, int id);
    Task<IEnumerable<object>> Get(QualityType quality, ItemInfoTypeEnum type);

    Task<object> GetWeapon(QualityType quality, int id, ItemInfoTypeEnum type);
    Task<IEnumerable<object>> GetWeapons(QualityType quality);
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

	public async Task<int> Count()
	{
		var subItemRepository = await _subItemRepositoryFactory.Create();
		var uniqueRepository = await _uniqueRepositoryFactory.Create();
		var setRepository = await _setRepositoryFactory.Create();
		
		var subItemsCount = subItemRepository.GetAll().Count();
		var uniquesCount = uniqueRepository.GetAll().Count();
		var setsCount = setRepository.GetAll().Count();
		return subItemsCount + uniquesCount + setsCount;
	}
}
