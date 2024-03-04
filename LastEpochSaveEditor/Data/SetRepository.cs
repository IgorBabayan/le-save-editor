using LastEpochSaveEditor.Models.Database;

namespace LastEpochSaveEditor.Data;

public class SetRepository : Repository<Unique>
{
	#region IRepository<TEntity>

	public override Unique Get(ItemInfoTypeEnum itemType, int id)
	{
		var result = Get(itemType);
		return result.FirstOrDefault(x => x.SetID == id && x.IsSetItem)
			?? throw new ArgumentException(null, nameof(itemType));
	}

	public override IEnumerable<Unique> Get(ItemInfoTypeEnum itemType)
	{
		var result = _database.Uniques.Where(x => x.BaseType == itemType && x.IsSetItem);
		return result;
	}

	public override IEnumerable<Unique> GetAll()
	{
		var result = _database.Uniques.Where(x => x.IsSetItem);
		return result;
	}

	#endregion

	public SetRepository(Database database)
		: base(database) { }
}