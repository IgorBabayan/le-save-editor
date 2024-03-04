namespace LastEpochSaveEditor.Services;

public interface IItemInfo<TEnum>
	where TEnum : struct, Enum
{
	string BaseIcon { get; }
	int Width { get; }
	int Height { get; }
	TEnum Type { get; }
	Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType);
}

public class HelmItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Helmet;
	public string BaseIcon => Const.HELM_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.HELMETS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class BodyItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Body;
	public string BaseIcon => Const.BODY_ARMOR_ICON;
	public int Width => 93;
	public int Height => 139;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.BODY_ARMOR}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class WeaponItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Weapons;
	public string BaseIcon => Const.WEAPON_ICON;
	public int Width => 63;
	public int Height => 139;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.GetWeapon(quality, id, itemType);
		var path = $"{item.GetWeaponTypePath()}/{itemType.GetDescriptionName()}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class OffHandItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.OffHands;
	public string BaseIcon => Const.OFF_HAND_ICON;
	public int Width => 69;
	public int Height => 139;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.OFF_HAND}/{itemType}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class GlovesItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Gloves;
	public string BaseIcon => Const.GLOVES_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.GLOVES}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class BeltItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Belt;
	public string BaseIcon => Const.BELTS_ICON;
	public int Width => 93;
	public int Height => 39;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.BELTS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class BootsItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Boots;
	public string BaseIcon => Const.BOOTS_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.BOOTS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class RingItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Ring;
	public string BaseIcon => Const.RING_ICON;
	public int Width => 39;
	public int Height => 39;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.RING}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class AmuletItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Amulet;
	public string BaseIcon => Const.AMULET_ICON;
	public int Width => 43;
	public int Height => 43;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.AMULET}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class RelicItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Relic;
	public string BaseIcon => Const.RELIC_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{Const.RELIC}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}