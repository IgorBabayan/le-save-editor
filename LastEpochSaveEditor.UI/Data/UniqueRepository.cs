namespace LastEpochSaveEditor.Data;

public class UniqueRepository : Repository<Unique>
{
	#region IRepository<TEntity>

	public override Unique Get(ItemInfoTypeEnum itemType, int id)
	{
		var result = Get(itemType);
		return result.FirstOrDefault(x => x.UniqueID == id && !x.IsSetItem)
			?? throw new ArgumentException(null, nameof(itemType));
	}

	public override IEnumerable<Unique> Get(ItemInfoTypeEnum itemType)
	{
		switch (itemType)
		{
			case ItemInfoTypeEnum.OneHandWeapons:
				return _database.Uniques.Where(x => x.BaseType.IsOneHands() && !x.IsSetItem);

			case ItemInfoTypeEnum.TwoHandWeapons:
				return _database.Uniques.Where(x => x.BaseType.IsTwoHands() && !x.IsSetItem);

			case ItemInfoTypeEnum.Weapons:
				return _database.Uniques.Where(x => x.BaseType.IsWeapons() && !x.IsSetItem);

			case ItemInfoTypeEnum.Accessories:
				return _database.Uniques.Where(x => x.BaseType.IsAccessories() && !x.IsSetItem);

			case ItemInfoTypeEnum.Armours:
				return _database.Uniques.Where(x => x.BaseType.IsArmours() && !x.IsSetItem);

			case ItemInfoTypeEnum.OffHands:
				return _database.Uniques.Where(x => x.BaseType.IsOffHands() && !x.IsSetItem);

			case ItemInfoTypeEnum.Idols:
				return _database.Uniques.Where(x => x.BaseType.IsIdols() && !x.IsSetItem);

			default:
				return _database.Uniques.Where(x => x.BaseType == itemType && !x.IsSetItem);
		}
	}

	public override IEnumerable<Unique> GetAll()
	{
		var result = _database.Uniques.Where(x => !x.IsSetItem);
		return result;
	}

	public override IDictionary<ItemInfoTypeEnum, IEnumerable<Unique>> GetByType(ItemInfoTypeEnum itemType)
	{
		switch (itemType)
		{
			case ItemInfoTypeEnum.OneHandWeapons:
				return _database.Uniques.Where(x => x.BaseType.IsOneHands() && !x.IsSetItem)
					.GroupBy(x => x.BaseType).ToDictionary(x => x.Key, x => x.AsEnumerable());

			case ItemInfoTypeEnum.TwoHandWeapons:
				return _database.Uniques.Where(x => x.BaseType.IsTwoHands() && !x.IsSetItem)
					.GroupBy(x => x.BaseType).ToDictionary(x => x.Key, x => x.AsEnumerable());

			case ItemInfoTypeEnum.Weapons:
				return _database.Uniques.Where(x => x.BaseType.IsWeapons() && !x.IsSetItem)
					.GroupBy(x => x.BaseType).ToDictionary(x => x.Key, x => x.AsEnumerable());

			case ItemInfoTypeEnum.Accessories:
				return _database.Uniques.Where(x => x.BaseType.IsAccessories() && !x.IsSetItem)
					.GroupBy(x => x.BaseType).ToDictionary(x => x.Key, x => x.AsEnumerable());

			case ItemInfoTypeEnum.Armours:
				return _database.Uniques.Where(x => x.BaseType.IsArmours() && !x.IsSetItem)
					.GroupBy(x => x.BaseType).ToDictionary(x => x.Key, x => x.AsEnumerable());

			case ItemInfoTypeEnum.OffHands:
				return _database.Uniques.Where(x => x.BaseType.IsOffHands() && !x.IsSetItem)
					.GroupBy(x => x.BaseType).ToDictionary(x => x.Key, x => x.AsEnumerable());

			case ItemInfoTypeEnum.Idols:
				return _database.Uniques.Where(x => x.BaseType.IsIdols() && !x.IsSetItem)
					.GroupBy(x => x.BaseType).ToDictionary(x => x.Key, x => x.AsEnumerable());
		}

		throw new ArgumentException(null, nameof(itemType));
	}

	public override IEnumerable<Unique> GetLens() => throw new ArgumentException($"Unsupported type {ItemInfoTypeEnum.Lens}");

	public override IEnumerable<Unique> GetMiscs() => throw new ArgumentException($"Unsupported type {ItemInfoTypeEnum.Misc}");

	#endregion

	public UniqueRepository(Database database)
		: base(database) { }
}
