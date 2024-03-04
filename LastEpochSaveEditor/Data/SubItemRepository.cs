namespace LastEpochSaveEditor.Data;

public class SubItemRepository : Repository<SubItem>
{
	#region IRepository<TEntity>

	public override SubItem Get(ItemInfoTypeEnum itemType, int id)
	{
		var result = Get(itemType);
		return result.FirstOrDefault(x => x.SubTypeID == id) ?? throw new ArgumentException(null, nameof(itemType));
	}

	public override IEnumerable<SubItem> Get(ItemInfoTypeEnum itemType)
	{
		var result = _database.ItemTypes.Where(x => x.BaseTypeID == itemType);
		return result.SelectMany(x => x.SubItems);
	}

	public override IEnumerable<SubItem> GetAll()
	{
		var result = _database.ItemTypes.SelectMany(x => x.SubItems);
		return result;
	}

	#endregion

	public SubItemRepository(Database database)
		: base(database) { }
}