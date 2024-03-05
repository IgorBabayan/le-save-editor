namespace LastEpochSaveEditor.Factories;

public interface IItemInfoFactory
{
	IItemInfo<ItemInfoTypeEnum> Create(ItemInfoTypeEnum type);
}

public class ItemInfoFactory : IItemInfoFactory
{
	private readonly Func<IEnumerable<IItemInfo<ItemInfoTypeEnum>>> _factory;

	public IItemInfo<ItemInfoTypeEnum> Create(ItemInfoTypeEnum type)
	{
		var factories = _factory();
		switch (type)
		{
			case ItemInfoTypeEnum.OneHandWeapons:
			case ItemInfoTypeEnum.TwoHandWeapons:
			case ItemInfoTypeEnum.Weapons:
			case ItemInfoTypeEnum.Bows:
			case ItemInfoTypeEnum.Crossbow:
			case ItemInfoTypeEnum.OneHandAxes:
			case ItemInfoTypeEnum.Daggers:
			case ItemInfoTypeEnum.OneHandMaces:
			case ItemInfoTypeEnum.OneHandScepter:
			case ItemInfoTypeEnum.OneHandSwords:
			case ItemInfoTypeEnum.Wands:
			case ItemInfoTypeEnum.TwoHandAxes:
			case ItemInfoTypeEnum.TwoHandMaces:
			case ItemInfoTypeEnum.TwoHandSwords:
			case ItemInfoTypeEnum.TwoHandPolearm:
			case ItemInfoTypeEnum.Staff:
				return factories.First(x => x.GetType() == typeof(WeaponItemInfo));

			case ItemInfoTypeEnum.Catalyst:
			case ItemInfoTypeEnum.Quiver:
			case ItemInfoTypeEnum.Shield:
				return factories.First(x => x.GetType() == typeof(OffHandItemInfo));

			default:
				return factories.First(x => x.Type == type);
		}
	}

	public ItemInfoFactory(Func<IEnumerable<IItemInfo<ItemInfoTypeEnum>>> factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));
}
