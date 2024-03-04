using LastEpochSaveEditor.Models.Database;

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
		switch (itemType)
		{
			case ItemInfoTypeEnum.OneHandWeapons:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsOneHands()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.TwoHandWeapons:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsTwoHands()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Weapons:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInWeapons()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Accessories:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInAccessories()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Armours:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInArmours()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.OffHands:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInOffHands()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Idols:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInIdols()).SelectMany(x => x.SubItems);

			default:
				return _database.ItemTypes.Where(x => x.BaseTypeID == itemType).SelectMany(x => x.SubItems);
		}
	}

	public override IEnumerable<SubItem> GetAll()
	{
		var result = _database.ItemTypes.SelectMany(x => x.SubItems);
		return result;
	}

	public override IDictionary<ItemInfoTypeEnum, IEnumerable<SubItem>> GetByType(ItemInfoTypeEnum type)
	{
		switch (type)
		{
			case ItemInfoTypeEnum.OneHandWeapons:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsOneHands()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.TwoHandWeapons:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsTwoHands()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Weapons:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInWeapons()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Accessories:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInAccessories()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Armours:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInArmours()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.OffHands:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInOffHands()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Idols:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsInIdols()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());
		}

		throw new ArgumentException(null, nameof(type));
	}

	#endregion

	public SubItemRepository(Database database)
		: base(database) { }
}