namespace LastEpochSaveEditor.Data;

public abstract class Repository<TEntity> : IRepository<TEntity>
	where TEntity : class
{
	protected readonly Database _database;

	#region IEnumerableRepository<TEntity>

	public IEnumerable<TEntity> GetOneHandAxes() => Get(ItemInfoTypeEnum.OneHandAxes);
	public IEnumerable<TEntity> GetOneHandDaggers() => Get(ItemInfoTypeEnum.OneHandDaggers);
	public IEnumerable<TEntity> GetOneHandMaces() => Get(ItemInfoTypeEnum.OneHandMaces);
	public IEnumerable<TEntity> GetOneHandScepters() => Get(ItemInfoTypeEnum.OneHandScepter);
	public IEnumerable<TEntity> GetOneHandSwords() => Get(ItemInfoTypeEnum.OneHandSwords);
	public IEnumerable<TEntity> GetWands() => Get(ItemInfoTypeEnum.Wands);
	public IEnumerable<TEntity> GetTwoHandAxes() => Get(ItemInfoTypeEnum.TwoHandAxes);
	public IEnumerable<TEntity> GetTwoHandMaces() => Get(ItemInfoTypeEnum.TwoHandMaces);
	public IEnumerable<TEntity> GetTwoHandSpears() => Get(ItemInfoTypeEnum.TwoHandPolearm);
	public IEnumerable<TEntity> GetStaffs() => Get(ItemInfoTypeEnum.Staff);
	public IEnumerable<TEntity> GetBows() => Get(ItemInfoTypeEnum.Bows)
		.Union(Get(ItemInfoTypeEnum.Crossbow));
	public IEnumerable<TEntity> GetTwoHandSwords() => Get(ItemInfoTypeEnum.TwoHandSwords);
	public IEnumerable<TEntity> GetAmulets() => Get(ItemInfoTypeEnum.Amulet);
	public IEnumerable<TEntity> GetBodies() => Get(ItemInfoTypeEnum.Body);
	public IEnumerable<TEntity> GetBelts() => Get(ItemInfoTypeEnum.Belt);
	public IEnumerable<TEntity> GetBoots() => Get(ItemInfoTypeEnum.Boots);
	public IEnumerable<TEntity> GetGloves() => Get(ItemInfoTypeEnum.Gloves);
	public IEnumerable<TEntity> GetHelmets() => Get(ItemInfoTypeEnum.Helmet);
	public IEnumerable<TEntity> GetRelics() => Get(ItemInfoTypeEnum.Relic);
	public IEnumerable<TEntity> GetRings() => Get(ItemInfoTypeEnum.Ring);
	public IEnumerable<TEntity> GetQuivers() => Get(ItemInfoTypeEnum.Quiver);
	public IEnumerable<TEntity> GetShields() => Get(ItemInfoTypeEnum.Shield);
	public IEnumerable<TEntity> GetCatalysts() => Get(ItemInfoTypeEnum.Catalyst);
	public IEnumerable<TEntity> GetSmallIdols() => Get(ItemInfoTypeEnum.SmallIdol);
	public IEnumerable<TEntity> GetSmallLagonianIdols() => Get(ItemInfoTypeEnum.SmallLagonianIdol);
	public IEnumerable<TEntity> GetHumbleIdols() => Get(ItemInfoTypeEnum.HumbleIdol);
	public IEnumerable<TEntity> GetStoutIdols() => Get(ItemInfoTypeEnum.StoutIdol);
	public IEnumerable<TEntity> GetGrandIdols() => Get(ItemInfoTypeEnum.GrandIdol);
	public IEnumerable<TEntity> GetLargeIdols() => Get(ItemInfoTypeEnum.LargeIdol);
	public IEnumerable<TEntity> GetOrnateIdols() => Get(ItemInfoTypeEnum.OrnateIdol);
	public IEnumerable<TEntity> GetHugeIdols() => Get(ItemInfoTypeEnum.HugeIdol);
	public IEnumerable<TEntity> GetAdornedIdols() => Get(ItemInfoTypeEnum.AdornedIdol);
	public IEnumerable<TEntity> GetOneHandWeapons() => GetOneHandAxes()
		.Union(GetOneHandDaggers())
		.Union(GetOneHandMaces())
		.Union(GetOneHandScepters())
		.Union(GetOneHandSwords())
		.Union(GetWands());
	public IEnumerable<TEntity> GetTwoHandWeapons() => GetTwoHandAxes()
		.Union(GetTwoHandMaces())
		.Union(GetTwoHandSwords())
		.Union(GetTwoHandSpears())
		.Union(GetStaffs());
	public IEnumerable<TEntity> GetWeapons(ItemInfoTypeEnum type = ItemInfoTypeEnum.Weapons)
	{
		if (type == ItemInfoTypeEnum.None || type == ItemInfoTypeEnum.Weapons)
		{
			return GetOneHandWeapons()
				.Union(GetTwoHandWeapons())
				.Union(GetBows());
		}

		switch (type)
		{
			case ItemInfoTypeEnum.OneHandWeapons:
				return GetOneHandWeapons();

			case ItemInfoTypeEnum.TwoHandWeapons:
				return GetTwoHandWeapons();

			case ItemInfoTypeEnum.Bows:
				return GetBows();

			case ItemInfoTypeEnum.OneHandAxes:
				return GetOneHandAxes();

			case ItemInfoTypeEnum.OneHandDaggers:
				return GetOneHandDaggers();

			case ItemInfoTypeEnum.OneHandMaces:
				return GetOneHandMaces();

			case ItemInfoTypeEnum.OneHandScepter:
				return GetOneHandScepters();

			case ItemInfoTypeEnum.OneHandSwords:
				return GetOneHandSwords();

			case ItemInfoTypeEnum.Wands:
				return GetWands();

			case ItemInfoTypeEnum.TwoHandAxes:
				return GetTwoHandAxes();

			case ItemInfoTypeEnum.TwoHandMaces:
				return GetTwoHandMaces();

			case ItemInfoTypeEnum.TwoHandSwords:
				return GetTwoHandSwords();

			case ItemInfoTypeEnum.TwoHandPolearm:
				return GetTwoHandSpears();

			case ItemInfoTypeEnum.Staff:
				return GetStaffs();
		}

		throw new ArgumentException("Invalid weapon type", nameof(type));
	}

	#endregion

	#region ISingleRepository<TEntity>

	public TEntity GetOneHandAxe(int id) => Get(ItemInfoTypeEnum.OneHandAxes, id);
	public TEntity GetOneHandDagger(int id) => Get(ItemInfoTypeEnum.OneHandDaggers, id);
	public TEntity GetOneHandMace(int id) => Get(ItemInfoTypeEnum.OneHandMaces, id);
	public TEntity GetOneHandScepter(int id) => Get(ItemInfoTypeEnum.OneHandScepter, id);
	public TEntity GetOneHandSword(int id) => Get(ItemInfoTypeEnum.OneHandSwords, id);
	public TEntity GetWand(int id) => Get(ItemInfoTypeEnum.Wands, id);
	public TEntity GetTwoHandAxe(int id) => Get(ItemInfoTypeEnum.TwoHandAxes, id);
	public TEntity GetTwoHandMace(int id) => Get(ItemInfoTypeEnum.TwoHandMaces, id);
	public TEntity GetTwoHandSpear(int id) => Get(ItemInfoTypeEnum.TwoHandPolearm, id);
	public TEntity GetStaff(int id) => Get(ItemInfoTypeEnum.Staff, id);
	public TEntity GetBow(int id) => Get(ItemInfoTypeEnum.Bows, id);
	public TEntity GetTwoHandSword(int id) => Get(ItemInfoTypeEnum.TwoHandSwords, id);
	public TEntity GetAmulet(int id) => Get(ItemInfoTypeEnum.Amulet, id);
	public TEntity GetBody(int id) => Get(ItemInfoTypeEnum.Body, id);
	public TEntity GetBelt(int id) => Get(ItemInfoTypeEnum.Belt, id);
	public TEntity GetBoot(int id) => Get(ItemInfoTypeEnum.Boots, id);
	public TEntity GetGlove(int id) => Get(ItemInfoTypeEnum.Gloves, id);
	public TEntity GetHelmet(int id) => Get(ItemInfoTypeEnum.Helmet, id);
	public TEntity GetRelic(int id) => Get(ItemInfoTypeEnum.Relic, id);
	public TEntity GetRing(int id) => Get(ItemInfoTypeEnum.Ring, id);
	public TEntity GetQuiver(int id) => Get(ItemInfoTypeEnum.Quiver, id);
	public TEntity GetShield(int id) => Get(ItemInfoTypeEnum.Shield, id);
	public TEntity GetCatalyst(int id) => Get(ItemInfoTypeEnum.Catalyst, id);
	public TEntity GetSmallIdol(int id) => Get(ItemInfoTypeEnum.SmallIdol, id);
	public TEntity GetSmallLagonianIdol(int id) => Get(ItemInfoTypeEnum.SmallLagonianIdol, id);
	public TEntity GetHumbleIdol(int id) => Get(ItemInfoTypeEnum.HumbleIdol, id);
	public TEntity GetStoutIdol(int id) => Get(ItemInfoTypeEnum.StoutIdol, id);
	public TEntity GetGrandIdol(int id) => Get(ItemInfoTypeEnum.GrandIdol, id);
	public TEntity GetLargeIdol(int id) => Get(ItemInfoTypeEnum.LargeIdol, id);
	public TEntity GetOrnateIdol(int id) => Get(ItemInfoTypeEnum.OrnateIdol, id);
	public TEntity GetHugeIdol(int id) => Get(ItemInfoTypeEnum.HugeIdol, id);
	public TEntity GetAdornedIdol(int id) => Get(ItemInfoTypeEnum.AdornedIdol, id);
	public WeaponType<TEntity> GetWeapon(int id, ItemInfoTypeEnum type)
	{
		switch (type)
		{
			case ItemInfoTypeEnum.OneHandAxes:
				return new WeaponType<TEntity>(false, GetOneHandAxe(id));

			case ItemInfoTypeEnum.OneHandDaggers:
				return new WeaponType<TEntity>(false, GetOneHandDagger(id));

			case ItemInfoTypeEnum.OneHandMaces:
				return new WeaponType<TEntity>(false, GetOneHandMace(id));

			case ItemInfoTypeEnum.OneHandScepter:
				return new WeaponType<TEntity>(false, GetOneHandScepter(id));

			case ItemInfoTypeEnum.OneHandSwords:
				return new WeaponType<TEntity>(false, GetOneHandSword(id));

			case ItemInfoTypeEnum.Wands:
				return new WeaponType<TEntity>(false, GetWand(id));

			case ItemInfoTypeEnum.TwoHandAxes:
				return new WeaponType<TEntity>(true, GetTwoHandAxe(id));

			case ItemInfoTypeEnum.TwoHandMaces:
				return new WeaponType<TEntity>(true, GetTwoHandMace(id));

			case ItemInfoTypeEnum.TwoHandSwords:
				return new WeaponType<TEntity>(true, GetTwoHandSword(id));

			case ItemInfoTypeEnum.TwoHandPolearm:
				return new WeaponType<TEntity>(true, GetTwoHandSpear(id));

			case ItemInfoTypeEnum.Staff:
				return new WeaponType<TEntity>(true, GetStaff(id));

			case ItemInfoTypeEnum.Bows:
				return new WeaponType<TEntity>(true, GetBow(id));
		}

		throw new ArgumentException("Invalid weapon type", nameof(type));
	}

	#endregion

	#region IRepository<TEntity>

	public abstract TEntity Get(ItemInfoTypeEnum itemType, int id);

	public abstract IEnumerable<TEntity> Get(ItemInfoTypeEnum itemType);

	public abstract IEnumerable<TEntity> GetAll();

	#endregion

	protected Repository(Database database) => _database = database;
}
