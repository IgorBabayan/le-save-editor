namespace LastEpochSaveEditor.Services.ItemInfo;

public class WeaponItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Weapons;
	public string BaseIcon => WEAPON_ICON;
	public int Width => 63;
	public int Height => 139;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.GetWeapon(quality, id, itemType);
		var path = $"{item.GetWeaponTypePath()}/{itemType.GetDescriptionName()}/{quality.GetQualityFolderName()}/{item.GetName()}.png";
		return Path.GetFullPath(path);
	}
}
