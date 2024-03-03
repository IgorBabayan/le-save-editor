using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Reflection;

namespace LastEpochSaveEditor.Extensions;

internal static class EnumExtensions
{
	private static readonly IEnumerable<ItemInfoTypeEnum> _weapons;
	private static readonly IEnumerable<ItemInfoTypeEnum> _offHands;

	static EnumExtensions()
	{
		_weapons = new[]
		{
			ItemInfoTypeEnum.Bows,
			ItemInfoTypeEnum.Crossbow,
			ItemInfoTypeEnum.OneHandAxes,
			ItemInfoTypeEnum.OneHandDaggers,
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
		_offHands = new[]
		{
			ItemInfoTypeEnum.Quiver,
			ItemInfoTypeEnum.Catalyst,
			ItemInfoTypeEnum.Shield
		};
	}

	public static bool IsInWeapon(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return _weapons.Any(x => x == self);
	}

	public static bool IsInOffHands(this ItemInfoTypeEnum self)
	{
		if (self == ItemInfoTypeEnum.None || self == ItemInfoTypeEnum.All)
			return false;

		return _offHands.Any(x => x == self);
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
