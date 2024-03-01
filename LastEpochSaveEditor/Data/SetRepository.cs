namespace LastEpochSaveEditor.Data;

public class SetRepository : Repository<Unique>
{
	#region IRepository<TEntity>

	public override Unique Get(ItemInfoTypeEnum itemType, int id)
	{
		var result = Get(itemType);
		return result.FirstOrDefault(x => x.SubTypes.Contains(id) && x.IsSetItem)
			?? throw new ArgumentException(nameof(itemType));
	}

	public override IEnumerable<Unique> Get(ItemInfoTypeEnum itemType)
	{
		var result = _database.Uniques.Where(x => x.BaseType == itemType);
		return result;
	}

	#endregion

	public SetRepository(Database database)
		: base(database) { }
}