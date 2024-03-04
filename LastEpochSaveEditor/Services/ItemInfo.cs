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
	public string BaseIcon => HELM_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{HELMETS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class BodyItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Body;
	public string BaseIcon => BODY_ARMOR_ICON;
	public int Width => 93;
	public int Height => 139;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{BODY_ARMOR}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class WeaponItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Weapons;
	public string BaseIcon => WEAPON_ICON;
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
	public string BaseIcon => OFF_HAND_ICON;
	public int Width => 69;
	public int Height => 139;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{OFF_HAND}/{itemType}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class GlovesItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Gloves;
	public string BaseIcon => GLOVES_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{GLOVES}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class BeltItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Belt;
	public string BaseIcon => BELTS_ICON;
	public int Width => 93;
	public int Height => 39;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{BELTS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class BootsItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Boots;
	public string BaseIcon => BOOTS_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{BOOTS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class RingItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Ring;
	public string BaseIcon => RING_ICON;
	public int Width => 39;
	public int Height => 39;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{RING}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class AmuletItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Amulet;
	public string BaseIcon => AMULET_ICON;
	public int Width => 43;
	public int Height => 43;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{AMULET}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class RelicItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Relic;
	public string BaseIcon => RELIC_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(DatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{RELIC}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}