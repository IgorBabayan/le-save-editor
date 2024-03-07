namespace LastEpochSaveEditor.Services.ItemInfo;

public class BootsItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Boots;
	public string BaseIcon => BOOTS_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{BOOTS}/{quality.GetQualityFolderName()}/{item.GetName()}.png";
		return Path.GetFullPath(path);
	}
}
