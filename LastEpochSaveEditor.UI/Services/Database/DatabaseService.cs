namespace LastEpochSaveEditor.Services.Database;

public class DatabaseService(
	ISubItemRepositoryFactory _subItemFactory,
	IUniqueRepositoryFactory _uniqueFactory,
	ISetRepositoryFactory _setFactory)
	: IDatabaseService
{
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
				subItemRepository = await _subItemFactory.Create();
				return subItemRepository.Get(type, id);

			case QualityType.Unique:
			case QualityType.Legendary:
				uniqueRepository = await _uniqueFactory.Create();
				return uniqueRepository.Get(type, id);

			case QualityType.Set:
				uniqueRepository = await _setFactory.Create();
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
				subItemRepository = await _subItemFactory.Create();
				return subItemRepository.Get(type);

			case QualityType.Unique:
			case QualityType.Legendary:
				uniqueRepository = await _uniqueFactory.Create();
				return uniqueRepository.Get(type);

			case QualityType.Set:
				uniqueRepository = await _setFactory.Create();
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
				subItemRepository = await _subItemFactory.Create();
				return subItemRepository.GetWeapon(id, type);

			case QualityType.Unique:
			case QualityType.Legendary:
				uniqueRepository = await _uniqueFactory.Create();
				return uniqueRepository.GetWeapon(id, type);

			case QualityType.Set:
				uniqueRepository = await _setFactory.Create();
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
				subItemRepository = await _subItemFactory.Create();
				return subItemRepository.GetWeapons();

			case QualityType.Unique:
			case QualityType.Legendary:
				uniqueRepository = await _uniqueFactory.Create();
				return uniqueRepository.GetWeapons();

			case QualityType.Set:
				uniqueRepository = await _setFactory.Create();
				return uniqueRepository.GetWeapons();

			default:
				throw new InvalidOperationException("Could not find any weapons");
		}
	}

	public async Task<int> Count()
	{
		var subItemRepository = await _subItemFactory.Create();
		var uniqueRepository = await _uniqueFactory.Create();
		var setRepository = await _setFactory.Create();

		var subItemsCount = subItemRepository.GetAll().Count();
		var uniquesCount = uniqueRepository.GetAll().Count();
		var setsCount = setRepository.GetAll().Count();
		return subItemsCount + uniquesCount + setsCount;
	}
}