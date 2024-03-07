namespace LastEpochSaveEditor.Services.ItemInfo;

public class RingItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Ring;
	public string BaseIcon => RING_ICON;
	public int Width => 39;
	public int Height => 39;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{RING}/{quality.GetQualityFolderName()}/{item.GetName()}.png";
		return Path.GetFullPath(path);
	}
}
