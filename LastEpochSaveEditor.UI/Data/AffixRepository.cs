namespace LastEpochSaveEditor.Data;

public class AffixRepository : Repository<Affix>
{
	#region IRepository<TEntity>

	public override Affix Get(ItemInfoTypeEnum itemType, int id)
	{
		var result = Get(itemType);
		return result.FirstOrDefault(x => x.Group == id) ?? throw new ArgumentException(null, nameof(itemType));
	}

	public override IEnumerable<Affix> Get(ItemInfoTypeEnum itemType) => GetAll();

	public override IEnumerable<Affix> GetAll() => _database.Affixes;

	public override IDictionary<ItemInfoTypeEnum, IEnumerable<Affix>> GetByType(ItemInfoTypeEnum type) =>
		throw new ArgumentException($"Unsupported type {ItemInfoTypeEnum.Misc}");

	public override IEnumerable<Affix> GetLens() => throw new ArgumentException($"Unsupported type {ItemInfoTypeEnum.Lens}");

	public override IEnumerable<Affix> GetMiscs() => throw new ArgumentException($"Unsupported type {ItemInfoTypeEnum.Misc}");

	#endregion

	public AffixRepository(Database database)
		: base(database) { }
}