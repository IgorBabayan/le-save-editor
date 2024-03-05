namespace LastEpochSaveEditor.Extensions;

internal static class EnumExtensions
{
	public static readonly IEnumerable<ItemInfoTypeEnum> Weapons;
	public static readonly IEnumerable<ItemInfoTypeEnum> OffHands;
	public static readonly IEnumerable<ItemInfoTypeEnum> Accessories;
	public static readonly IEnumerable<ItemInfoTypeEnum> Armours;
	public static readonly IEnumerable<ItemInfoTypeEnum> Idols;
	public static readonly IEnumerable<ItemInfoTypeEnum> OneHands;
	public static readonly IEnumerable<ItemInfoTypeEnum> TwoHands;
	public static readonly IEnumerable<ItemInfoTypeEnum> Lens;
	public static readonly IEnumerable<ItemInfoTypeEnum> Misc;

	static EnumExtensions()
	{
		Weapons = new[]
		{
			ItemInfoTypeEnum.Bows,
			ItemInfoTypeEnum.Crossbow,
			ItemInfoTypeEnum.OneHandAxes,
			ItemInfoTypeEnum.Daggers,
			ItemInfoTypeEnum.OneHandMaces,
			ItemInfoTypeEnum.OneHandScepter,
			ItemInfoTypeEnum.OneHandSwords,
			ItemInfoTypeEnum.OneHandFist,
			ItemInfoTypeEnum.Wands,
			ItemInfoTypeEnum.TwoHandAxes,
			ItemInfoTypeEnum.TwoHandMaces,
			ItemInfoTypeEnum.TwoHandSwords,
			ItemInfoTypeEnum.TwoHandPolearm,
			ItemInfoTypeEnum.Staff
		};
		OffHands = new[]
		{
			ItemInfoTypeEnum.Quiver,
			ItemInfoTypeEnum.Shield,
			ItemInfoTypeEnum.Catalyst
		};
		Accessories = new[]
		{
			ItemInfoTypeEnum.Amulet,
			ItemInfoTypeEnum.Ring,
			ItemInfoTypeEnum.Relic
		};
		Armours = new[]
		{
			ItemInfoTypeEnum.Helmet,
			ItemInfoTypeEnum.Body,
			ItemInfoTypeEnum.Belt,
			ItemInfoTypeEnum.Boots,
			ItemInfoTypeEnum.Gloves
		};
		Idols = new[]
		{
			ItemInfoTypeEnum.SmallIdol,
			ItemInfoTypeEnum.SmallLagonianIdol,
			ItemInfoTypeEnum.HumbleIdol,
			ItemInfoTypeEnum.StoutIdol,
			ItemInfoTypeEnum.GrandIdol,
			ItemInfoTypeEnum.LargeIdol,
			ItemInfoTypeEnum.OrnateIdol,
			ItemInfoTypeEnum.HugeIdol,
			ItemInfoTypeEnum.AdornedIdol
		};
		OneHands = new[]
		{
			ItemInfoTypeEnum.OneHandAxes,
			ItemInfoTypeEnum.Daggers,
			ItemInfoTypeEnum.OneHandMaces,
			ItemInfoTypeEnum.OneHandScepter,
			ItemInfoTypeEnum.OneHandSwords,
			ItemInfoTypeEnum.Wands
		};
		TwoHands = new[]
		{
			ItemInfoTypeEnum.TwoHandAxes,
			ItemInfoTypeEnum.TwoHandMaces,
			ItemInfoTypeEnum.TwoHandPolearm,
			ItemInfoTypeEnum.Staff,
			ItemInfoTypeEnum.TwoHandSwords,
			ItemInfoTypeEnum.Bows
		};
		Lens = new[]
		{
			ItemInfoTypeEnum.GreaterLens,
			ItemInfoTypeEnum.ArctusLens,
			ItemInfoTypeEnum.MesembriaLens,
			ItemInfoTypeEnum.EosLens,
			ItemInfoTypeEnum.DysisLens
		};
		Misc = new[]
		{
			ItemInfoTypeEnum.Blessing,
			ItemInfoTypeEnum.Rune,
			ItemInfoTypeEnum.Glyph,
			ItemInfoTypeEnum.Key,
			ItemInfoTypeEnum.LostMemory,
			ItemInfoTypeEnum.Resonance
		};
	}

	public static bool IsMiscs(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return Misc.Any(x => x == self);
	}

	public static bool IsLens(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return Lens.Any(x => x == self);
	}

	public static bool IsWeapons(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return Weapons.Any(x => x == self);
	}

	public static bool IsOffHands(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return OffHands.Any(x => x == self);
	}

	public static bool IsAccessories(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return Accessories.Any(x => x == self);
	}

	public static bool IsArmours(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return Armours.Any(x => x == self);
	}

	public static bool IsIdols(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return Idols.Any(x => x == self);
	}

	public static bool IsOneHands(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return OneHands.Any(x => x == self);
	}

	public static bool IsTwoHands(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return TwoHands.Any(x => x == self);
	}

	public static string GetDescriptionName(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All || self == ItemInfoTypeEnum.Weapons)
			throw new ArgumentException(null, nameof(self));

		var field = self.GetType().GetField(self.ToString());
		var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
		return attribute != null ? attribute.Description : self.ToString();
	}
}
