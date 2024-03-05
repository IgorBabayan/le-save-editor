namespace LastEpochSaveEditor.Services.ItemInfo;

public class BeltItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Belt;
	public string BaseIcon => BELTS_ICON;
	public int Width => 93;
	public int Height => 39;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{BELTS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}
