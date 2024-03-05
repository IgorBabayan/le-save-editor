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
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsWeapons()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Accessories:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsAccessories()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Armours:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsArmours()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.OffHands:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsOffHands()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Idols:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsIdols()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Lens:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsLens()).SelectMany(x => x.SubItems);

			case ItemInfoTypeEnum.Misc:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsMiscs()).SelectMany(x => x.SubItems);

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
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsWeapons()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Accessories:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsAccessories()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Armours:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsArmours()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.OffHands:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsOffHands()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Idols:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsIdols()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Lens:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsLens()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());

			case ItemInfoTypeEnum.Misc:
				return _database.ItemTypes.Where(x => x.BaseTypeID.IsMiscs()).ToDictionary(x => x.BaseTypeID, x => x.SubItems.AsEnumerable());
		}

		throw new ArgumentException(null, nameof(type));
	}

	public override IEnumerable<SubItem> GetLens() => Get(ItemInfoTypeEnum.Lens);

	public override IEnumerable<SubItem> GetMiscs() => Get(ItemInfoTypeEnum.Misc);

	#endregion

	public SubItemRepository(Database database)
		: base(database) { }
}